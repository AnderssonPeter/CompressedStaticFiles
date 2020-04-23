using CompressedStaticFiles.CompressionTypes;
using Microsoft.AspNetCore.Builder;

namespace CompressedStaticFiles
{
    public class CompressedStaticFileOptions : StaticFileOptions
    {
        public CompressionTypeCollection CompressionTypes { get; } = new CompressionTypeCollection{new Gzip(),new Brotli()};
    }
}