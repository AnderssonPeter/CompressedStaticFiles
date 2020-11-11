using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
#if NETSTANDARD2_0
#else
using IHost = Microsoft.AspNetCore.Hosting.IWebHostEnvironment;
#endif


namespace CompressedStaticFiles
{
    public interface IAlternativeFileProvider
    {
        void Initialize(FileExtensionContentTypeProvider fileExtensionContentTypeProvider);
        IFileAlternative GetAlternative(HttpContext context, IFileProvider fileSystem, IFileInfo originalFile);
    }
}
