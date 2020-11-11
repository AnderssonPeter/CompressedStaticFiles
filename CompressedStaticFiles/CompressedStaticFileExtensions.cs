using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace CompressedStaticFiles
{
    public static class CompressedStaticFileExtensions
    {
        public static CompressedStaticFileOptions RemoveImageSubstitutionCostRatio(this CompressedStaticFileOptions compressedStaticFileOptions)
        {
            compressedStaticFileOptions.ImageSubstitutionCostRatio.Clear();
            return compressedStaticFileOptions;
        }

        public static IServiceCollection AddCompressedStaticFiles(this IServiceCollection services)
        {
            services.AddSingleton<IAlternativeFileProvider, CompressedAlternativeFileProvider>();
            services.AddSingleton<IAlternativeFileProvider, AlternativeImageFileProvider>();
            return services;
        }

        public static IServiceCollection AddCompressedStaticFiles(this IServiceCollection services, Action<CompressedStaticFileOptions> configureOptions)
        {
            services.Configure(configureOptions);
            services.AddSingleton<IAlternativeFileProvider, CompressedAlternativeFileProvider>();
            services.AddSingleton<IAlternativeFileProvider, AlternativeImageFileProvider>();
            return services;
        }

        public static IApplicationBuilder UseCompressedStaticFiles(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<CompressedStaticFileMiddleware>();
        }

        public static IApplicationBuilder UseCompressedStaticFiles(this IApplicationBuilder app, StaticFileOptions staticFileOptions)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<CompressedStaticFileMiddleware>(Options.Create(staticFileOptions));
        }
    }
}
