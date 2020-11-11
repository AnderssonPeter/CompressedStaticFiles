using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CompressedStaticFiles
{
    public class AlternativeImageFileProvider : IAlternativeFileProvider
    {

        private static Dictionary<string, string[]> imageFormats =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "image/avif", new [] { ".avif" } },
                { "image/webp", new [] { ".webp" } },
                { "image/jpeg", new [] { ".jpg", ".jpeg", ".jfif", ".pjpeg", ".pjp" } },
                { "image/png", new [] { ".png" } },
                { "image/bmp", new [] { ".bmp" } },
                { "image/apng", new [] { ".apng" } },
                { "image/gif", new [] { ".gif" } },
                { "image/x-icon", new [] { ".ico", ".cur" } },
                { "image/tiff", new [] { ".tif", ".tiff" } }
            };
        private readonly ILogger logger;
        private readonly IOptions<CompressedStaticFileOptions> options;

        public AlternativeImageFileProvider(ILogger<AlternativeImageFileProvider> logger, IOptions<CompressedStaticFileOptions> options)
        {
            this.logger = logger;
            this.options = options;
        }

        public void Initialize(FileExtensionContentTypeProvider fileExtensionContentTypeProvider)
        {
            //Ensure that all image mime types are known!
            foreach (var mimeType in imageFormats.Keys)
            {
                foreach (var fileExtension in imageFormats[mimeType])
                {
                    if (!fileExtensionContentTypeProvider.Mappings.ContainsKey(fileExtension))
                    {
                        fileExtensionContentTypeProvider.Mappings.Add(fileExtension, mimeType);
                    }
                }
            }
        }

        private float GetCostRatioForFileExtension(string fileExtension)
        {
            foreach (var mimeType in imageFormats.Keys)
            {
                if (imageFormats[mimeType].Contains(fileExtension))
                {
                    if (options.Value.ImageSubstitutionCostRatio.TryGetValue(mimeType, out var cost))
                    {
                        return cost;
                    }
                    return 1;
                }
            }
            return 1;
        }


        private float GetCostRatioForPath(string path)
        {
            var fileExtension = Path.GetExtension(path);            
            return GetCostRatioForFileExtension(fileExtension);
        }

        public IFileAlternative GetAlternative(HttpContext context, IFileProvider fileSystem, IFileInfo originalFile)
        {
            if (!options.Value.EnableImageSubstitution)
            {
                return null;
            }

            var matchingFileExtensions = context.Request.Headers.GetCommaSeparatedValues("Accept")
                                                        .Where(mimeType => imageFormats.ContainsKey(mimeType))
                                                        .SelectMany(mimeType => imageFormats[mimeType]);

            var originalAlternativeImageFile = new AlternativeImageFile(logger, originalFile, originalFile, GetCostRatioForPath(originalFile.PhysicalPath));

            AlternativeImageFile matchedFile = originalAlternativeImageFile;
            var path = context.Request.Path.ToString();
            if (!path.Contains('.'))
            {
                return null;
            }
            var withoutExtension = path.Substring(0, path.LastIndexOf('.'));
            foreach (var fileExtension in matchingFileExtensions)
            {
                var file = fileSystem.GetFileInfo(withoutExtension + fileExtension);
                if (file.Exists)
                {
                    var alternativeFile = new AlternativeImageFile(logger, originalFile, file, GetCostRatioForFileExtension(fileExtension));
                    if (matchedFile.Cost > alternativeFile.Cost)
                    {
                        matchedFile = alternativeFile;
                    }
                }
            }

            if (matchedFile != originalAlternativeImageFile)
            {
                return matchedFile;
            }

            return null;
        }
    }
}
