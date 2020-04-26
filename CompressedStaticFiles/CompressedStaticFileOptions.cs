using CompressedStaticFiles.CompressionTypes;
using Microsoft.AspNetCore.Builder;

namespace CompressedStaticFiles
{
    public class CompressedStaticFileOptions : StaticFileOptions
    {
        public CompressionTypeCollection CompressionTypes { get; } = new CompressionTypeCollection{new Gzip(),new Brotli()};

        public CompressedStaticFileOptions() { }

        public CompressedStaticFileOptions(StaticFileOptions staticFileOptions)
        {
            this.ContentTypeProvider = staticFileOptions.ContentTypeProvider;
            this.DefaultContentType = staticFileOptions.DefaultContentType;
            this.FileProvider = staticFileOptions.FileProvider;
            this.OnPrepareResponse = staticFileOptions.OnPrepareResponse;
            this.RequestPath = staticFileOptions.RequestPath;
            this.ServeUnknownFileTypes = staticFileOptions.ServeUnknownFileTypes;
#if netcoreapp3_1
            this.HttpsCompression = staticFileOptions.HttpsCompression;
#endif
        }
    }
}