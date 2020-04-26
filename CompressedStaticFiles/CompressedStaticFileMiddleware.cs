using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
#if NETSTANDARD2_0
using IHost = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
#else
using IHost = Microsoft.AspNetCore.Hosting.IWebHostEnvironment;
#endif


namespace CompressedStaticFiles
{
    public class CompressedStaticFileMiddleware
    {
        private readonly IOptions<CompressedStaticFileOptions> _compressedStaticFileOptions;
        private readonly StaticFileMiddleware _base;
        private readonly ILogger _logger;

        public CompressedStaticFileMiddleware(
            RequestDelegate next,
            IHost hostingEnv,
            IOptions<StaticFileOptions> staticFileOptions, ILoggerFactory loggerFactory)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            if (hostingEnv == null)
            {
                throw new ArgumentNullException(nameof(hostingEnv));
            }

            _logger = loggerFactory.CreateLogger<CompressedStaticFileMiddleware>();

            var options = staticFileOptions ?? throw new ArgumentNullException(nameof(staticFileOptions));
            _compressedStaticFileOptions = GetCompressedStaticFileOptions(options);

            InitializeStaticFileOptions(hostingEnv, _compressedStaticFileOptions);

            _base = new StaticFileMiddleware(next, hostingEnv, _compressedStaticFileOptions, loggerFactory);
        }

        private static IOptions<CompressedStaticFileOptions> GetCompressedStaticFileOptions(IOptions<StaticFileOptions> staticFileOptions)
        {
            if (staticFileOptions.Value is CompressedStaticFileOptions compressedStaticFileOptions)
            {
                return Options.Create(compressedStaticFileOptions);
            }

            return Options.Create(new CompressedStaticFileOptions(staticFileOptions.Value));
        }

        private static void InitializeStaticFileOptions(IHost hostingEnv, IOptions<CompressedStaticFileOptions> compressedStaticFileOptions)
        {
            compressedStaticFileOptions.Value.FileProvider ??= hostingEnv.WebRootFileProvider;
            var contentTypeProvider = compressedStaticFileOptions.Value.ContentTypeProvider ?? new FileExtensionContentTypeProvider();
            if (contentTypeProvider is FileExtensionContentTypeProvider fileExtensionContentTypeProvider)
            {
                // the StaticFileProvider would not serve the file if it does not know the content-type
                foreach (var compressionType in compressedStaticFileOptions.Value.CompressionTypes)
                {
                    if (!fileExtensionContentTypeProvider.Mappings.ContainsKey(compressionType.Extension))
                    {
                        fileExtensionContentTypeProvider.Mappings[compressionType.Extension] = compressionType.ContentType;
                    }
                }
            }
            compressedStaticFileOptions.Value.ContentTypeProvider = contentTypeProvider;

            var originalPrepareResponse = compressedStaticFileOptions.Value.OnPrepareResponse;
            compressedStaticFileOptions.Value.OnPrepareResponse = ctx =>
            {
                originalPrepareResponse(ctx);
                foreach (var compressionType in compressedStaticFileOptions.Value.CompressionTypes)
                {
                    var fileExtension = compressionType.Extension;
                    if (ctx.File.Name.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        // we need to restore the original content type, otherwise it would be based on the compression type
                        // (for example "application/brotli" instead of "text/html")
                        if (contentTypeProvider.TryGetContentType(ctx.File.PhysicalPath.Remove(
                            ctx.File.PhysicalPath.Length - fileExtension.Length, fileExtension.Length), out var contentType))
                            ctx.Context.Response.ContentType = contentType;
                        ctx.Context.Response.Headers.Add("Content-Encoding", new[] { compressionType.Encoding });
                    }
                }
            };
        }

        public Task Invoke(HttpContext context)
        {
            if (context.Request.Path.HasValue)
            {
                ProcessRequest(context);
            }
            return _base.Invoke(context);
        }

        private void ProcessRequest(HttpContext context)
        {
            var fileSystem = _compressedStaticFileOptions.Value.FileProvider;
            var originalFile = fileSystem.GetFileInfo(context.Request.Path);

            if (!originalFile.Exists)
            {
                return;
            }

            var supportedEncodings = GetSupportedEncodings(context);

            // try to find a compressed version of the file and ensure that it is smaller than the uncompressed version
            IFileInfo matchedFile = originalFile;
            foreach (var compressionType in supportedEncodings)
            {
                var fileExtension = _compressedStaticFileOptions.Value.CompressionTypes.FirstOrDefault(c => c.Encoding == compressionType)?.Extension ?? string.Empty;
                var file = fileSystem.GetFileInfo(context.Request.Path + fileExtension);
                if (file.Exists && file.Length < matchedFile.Length)
                {
                    matchedFile = file;
                }
            }

            if (matchedFile != originalFile)
            {
                // a compressed version exists and is smaller, change the path to serve the compressed file
                var matchedPath = context.Request.Path.Value + Path.GetExtension(matchedFile.Name);
                _logger.LogFileServed(context.Request.Path.Value, matchedPath, originalFile.Length, matchedFile.Length);
                context.Request.Path = new PathString(matchedPath);
            }
        }

        /// <summary>
        /// Find the encodings that are supported by the browser and by this middleware
        /// </summary>
        private IEnumerable<string> GetSupportedEncodings(HttpContext context)
        {
            var browserSupportedCompressionTypes = context.Request.Headers["Accept-Encoding"].ToString().Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var validCompressionTypes = _compressedStaticFileOptions.Value.CompressionTypes.Encodings.Intersect(browserSupportedCompressionTypes, StringComparer.OrdinalIgnoreCase);
            return validCompressionTypes;
        }
    }
}
