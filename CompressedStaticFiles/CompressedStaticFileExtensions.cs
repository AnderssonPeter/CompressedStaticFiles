using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using System;

namespace CompressedStaticFiles
{
    public static class CompressedStaticFileExtensions
    {
        public static IApplicationBuilder UseCompressedStaticFiles(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<CompressedStaticFileMiddleware>();
        }


        public static IApplicationBuilder UseCompressedStaticFiles(this IApplicationBuilder app, CompressedStaticFileOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<CompressedStaticFileMiddleware>(Options.Create(options));
        }
    }
}
