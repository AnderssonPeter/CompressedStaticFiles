using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;

namespace CompressedStaticFiles
{
    public class CompressedAlternativeFile : IFileAlternative
    {
        private readonly ILogger logger;
        private readonly IFileInfo originalFile;
        private readonly IFileInfo alternativeFile;
        private readonly string[] virtualProviders;

        public CompressedAlternativeFile(ILogger logger, IFileInfo originalFile, IFileInfo alternativeFile, string[] virtualProviders = null)
        {
            this.logger = logger;
            this.originalFile = originalFile;
            this.alternativeFile = alternativeFile;
            this.virtualProviders = virtualProviders;
        }

        public long Size => alternativeFile.Length;

        public float Cost => Size;

        public void Apply(HttpContext context)
        {
            var matchedPath = context.Request.Path.Value + Path.GetExtension(alternativeFile.Name);
            logger.LogFileServed(context.Request.Path.Value, matchedPath, originalFile.Length, alternativeFile.Length);
            context.Request.Path = new PathString(matchedPath);
        }

        public void Prepare(IContentTypeProvider contentTypeProvider, StaticFileResponseContext staticFileResponseContext)
        {
            foreach (var compressionType in CompressedAlternativeFileProvider.CompressionTypes.Keys)
            {
                var fileExtension = CompressedAlternativeFileProvider.CompressionTypes[compressionType];

                //lets check for a different file provider first
                var fileType = staticFileResponseContext.File.GetType();

                //if it matches the alternate file provider, use request path instead of physical path and the code will work 
                //this can also be determined by simply physicalpath==null but we will still check for providers to keep 
                //things robust on different unknown use cases
                if (staticFileResponseContext.File.Name.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase) && 
                    staticFileResponseContext.File.PhysicalPath == null && virtualProviders?.Contains($"{fileType?.FullName}") == true)
                {
                    if (contentTypeProvider.TryGetContentType(staticFileResponseContext.Context.Request.Path.Value.Remove(
                    staticFileResponseContext.Context.Request.Path.Value.Length - fileExtension.Length, fileExtension.Length), out var providerContentType))
                        staticFileResponseContext.Context.Response.ContentType = providerContentType;
                    staticFileResponseContext.Context.Response.Headers.Add("Content-Encoding", new[] { compressionType });
                    //short-circuit
                    return;
                }

                if (staticFileResponseContext.File.Name.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase))
                {
                    // we need to restore the original content type, otherwise it would be based on the compression type
                    // (for example "application/brotli" instead of "text/html")
                    if (contentTypeProvider.TryGetContentType(staticFileResponseContext.File.PhysicalPath.Remove(
                        staticFileResponseContext.File.PhysicalPath.Length - fileExtension.Length, fileExtension.Length), out var contentType))
                        staticFileResponseContext.Context.Response.ContentType = contentType;
                    staticFileResponseContext.Context.Response.Headers.Add("Content-Encoding", new[] { compressionType });
                }
            }
        }
    }
}
