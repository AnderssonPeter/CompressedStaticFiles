using Microsoft.AspNetCore.Builder;
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
    }
}
