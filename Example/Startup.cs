using CompressedStaticFiles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Example
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCompressedStaticFiles();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDefaultFiles();
            app.UseDeveloperExceptionPage();
            app.UseCompressedStaticFiles();
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "wwwroot";
            });
        }
    }
}
