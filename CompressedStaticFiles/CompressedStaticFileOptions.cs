using System.Collections.Generic;

namespace CompressedStaticFiles
{
    public class CompressedStaticFileOptions
    {
        public bool EnablePrecompressedFiles { get; set; } = true;
        public bool EnableImageSubstitution { get; set; } = true;

        /// <summary>
        /// Used to prioritize image formats that contain higher quality per byte, if only size should be considered remove all entries.
        /// </summary>
        public Dictionary<string, float> ImageSubstitutionCostRatio { get; set; } = new Dictionary<string, float>()
        {
            { "image/bmp", 2 },
            { "image/tiff", 1 },
            { "image/gif", 1 },
            { "image/apng", 0.9f },
            { "image/png", 0.9f },
            { "image/webp", 0.9f },
            { "image/avif", 0.8f }
        };
    }
}
