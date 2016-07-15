using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CompressedStaticFiles
{
    public class CompressedStaticFileMiddleware
    {
        private static Dictionary<string, string> compressionTypes = new Dictionary<string, string>()
        {{"gzip", ".gz" },
         {"br", ".br" }};

        private IHostingEnvironment _hostingEnv;
        private StaticFileMiddleware _base;
        public CompressedStaticFileMiddleware(RequestDelegate next, IHostingEnvironment hostingEnv, IOptions<StaticFileOptions> options, ILoggerFactory loggerFactory)
        {
            _hostingEnv = hostingEnv;
            var contentTypeProvider = options.Value.ContentTypeProvider ?? new FileExtensionContentTypeProvider();
            options.Value.ContentTypeProvider = contentTypeProvider;
            options.Value.FileProvider = options.Value.FileProvider ?? hostingEnv.WebRootFileProvider;
            options.Value.OnPrepareResponse = ctx =>
            {
                foreach (var compressionType in compressionTypes.Keys)
                {
                    var fileExtension = compressionTypes[compressionType];
                    if (ctx.File.Name.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        string contentType = null;
                        if (contentTypeProvider.TryGetContentType(ctx.File.PhysicalPath.Remove(ctx.File.PhysicalPath.Length - fileExtension.Length, fileExtension.Length), out contentType))
                            ctx.Context.Response.ContentType = contentType;
                        ctx.Context.Response.Headers.Add("Content-Encoding", new[] { compressionType });
                    }
                }
            };

            _base = new StaticFileMiddleware(next, hostingEnv, options, loggerFactory);
        }

        public Task Invoke(HttpContext context)
        {
            if (context.Request.Path.HasValue)
            {
                string acceptEncoding = context.Request.Headers["Accept-Encoding"];
                FileInfo matchedFile = null;
                string[] browserSupportedCompressionTypes = context.Request.Headers["Accept-Encoding"].ToString().Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var compressionType in compressionTypes.Keys)
                {
                    if (browserSupportedCompressionTypes.Contains(compressionType, StringComparer.OrdinalIgnoreCase))
                    {
                        var fileExtension = compressionTypes[compressionType];
                        var filePath = System.IO.Path.Combine(
                                _hostingEnv.WebRootPath, context.Request.Path.Value.StartsWith("/")
                                ? context.Request.Path.Value.Remove(0, 1)
                                : context.Request.Path.Value
                            ) + fileExtension;
                        var file = new FileInfo(filePath);
                        if (file.Exists)
                        {
                            if (matchedFile == null)
                                matchedFile = file;
                            else if (matchedFile.Length > file.Length)
                                matchedFile = file;
                        }
                    }
                }
                if (matchedFile != null)
                {
                    context.Request.Path = new PathString(context.Request.Path.Value + matchedFile.Extension);
                    return _base.Invoke(context);
                }
            }
            return _base.Invoke(context);
        }
    }
}
