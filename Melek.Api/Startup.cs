using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melek.Api.Repositories;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Melek.Api
{
    public class Startup
    {
        private readonly IHostingEnvironment _HostingEnvironment;

        public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            // grab a reference to the hosting environment for later use
            _HostingEnvironment = env;
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            // configure DI
            services.AddSingleton<IMelekRepository, MelekRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseIISPlatformHandler();
            app.UseStaticFiles();
            app.UseMvc();

            // TODO: is this terrible?
            // configure MelekRepository singleton
            app.ApplicationServices.GetService<IMelekRepository>().SetDataSource(_HostingEnvironment.WebRootPath + @"\Data\melek-data-store.json");
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}