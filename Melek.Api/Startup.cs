using Melek.Api.Repositories.Implementations;
using Melek.Api.Repositories.Interfaces;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Framework.DependencyInjection;

namespace Melek.Api
{
    public class Startup
    {
        public Startup(IHostingEnvironment env) { }

        // Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            // di, maybe?
            services.AddSingleton<IMelekRepository, MelekRepository>();
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Configure the HTTP request pipeline.
            app.UseStaticFiles();

            // Add MVC to the request pipeline.
            app.UseMvc();

            // TODO: is this terrible?
            // configure MelekRepository singleton
            app.ApplicationServices.GetService<IMelekRepository>().SetDataSource(env.MapPath("Data/melek-data-store.json"));
        }
    }
}