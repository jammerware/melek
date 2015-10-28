using Melek.Api.Repositories.Implementations;
using Melek.Api.Repositories.Interfaces;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.DependencyInjection;

namespace Melek.Api
{
    public class Startup
    {
        private readonly IHostingEnvironment _HostingEnvironment;

        public Startup(IHostingEnvironment hostingEnvironment)
        {
            this._HostingEnvironment = hostingEnvironment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            // di, maybe?
            services.AddSingleton<IMelekRepository, MelekRepository>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseStaticFiles();
            app.UseMvc();

            // TODO: is this terrible?
            // configure MelekRepository singleton
            
            app.ApplicationServices.GetService<IMelekRepository>().SetDataSource(_HostingEnvironment.WebRootPath + @"\Data\melek-data-store.json");
        }
    }
}