using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using System.IO;

namespace CompressedStaticFiles
{
    public class AlternativeImageFile : IFileAlternative
    {
        private readonly ILogger logger;
        private readonly IFileInfo originalFile;
        private readonly IFileInfo alternativeFile;
        private readonly float costRatio;

        public AlternativeImageFile(ILogger logger, IFileInfo originalFile, IFileInfo alternativeFile, float costRatio)
        {
            this.logger = logger;
            this.originalFile = originalFile;
            this.alternativeFile = alternativeFile;
            this.costRatio = costRatio;
        }

        public long Size => alternativeFile.Length;

        public float Cost => Size * costRatio;

        public void Apply(HttpContext context)
        {            
            var path = context.Request.Path.Value;
            //Change file extension!
            var pathAndFilenameWithoutExtension = path.Substring(0, path.LastIndexOf('.'));
            var matchedPath = pathAndFilenameWithoutExtension + Path.GetExtension(alternativeFile.Name);
            logger.LogFileServed(context.Request.Path.Value, matchedPath, originalFile.Length, alternativeFile.Length);
            //Redirect the static file system to the alternative file
            context.Request.Path = new PathString(matchedPath);
            //Ensure that a caching proxy knows that it should cache based on the Accept header.
            context.Response.Headers.Add("Vary", "Accept");
        }

        public void Prepare(IContentTypeProvider contentTypeProvider, StaticFileResponseContext staticFileResponseContext)
        {
        }

    }
}
