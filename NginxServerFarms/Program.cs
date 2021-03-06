using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NginxServerFarms.Services;

namespace NginxServerFarms {
    public static class Program {
        public static async Task Main(string[] args) {
            try {
                var builder = CreateHostBuilder(args);
                using var host = builder.Build();

                await host.StartAsync().ConfigureAwait(false);

                // create hub client after building host, to ensure all hub dependencies are setup
                var client = (INginxHubClient)host.Services.GetService(typeof(INginxHubClient));
                await client.Connect("nginxHub").ConfigureAwait(false);

                await host.WaitForShutdownAsync().ConfigureAwait(false);
            } finally {
                WindowsProcessHelper.Stop();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
    }
}
