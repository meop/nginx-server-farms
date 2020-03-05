using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NginxServerFarms.Hubs;
using NginxServerFarms.Services;

namespace NginxServerFarms {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddControllersWithViews();

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration => {
                configuration.RootPath = "ClientApp/build";
            });

            var nginxConfigFileService = new NginxConfigFileService();
            var configNginxSection = this.Configuration.GetSection("Nginx");
            var serviceName = configNginxSection.GetValue<string>("ServiceName");
            var processName = configNginxSection.GetValue<string>("ProcessName");

            nginxConfigFileService.Watch(
                configNginxSection.GetValue<string>("ConfigFilePath"),
                configNginxSection.GetValue<int>("ConfigFileWatchDebounceTimeMs"));

            nginxConfigFileService.UpstreamsChangedEvent += (s, e) =>
                WindowsServiceHelper.ForceRestart(serviceName, processName);

            services.AddSingleton<INginxConfigFileService>(nginxConfigFileService);

            // adding as singleton ensures the connection stays active once constructed
            services.AddSingleton<INginxHubClient, NginxHubClient>();

            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            } else {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");

                endpoints.MapHub<NginxHub>("/nginxHub");
            });

            app.UseSpa(spa => {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment()) {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
