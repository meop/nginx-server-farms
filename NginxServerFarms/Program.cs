using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NginxServerFarms.Services;

namespace NginxServerFarms {
    public static class Program {
        public static void Main(string[] args) {
            var builder = CreateHostBuilder(args);
            var host = builder.Build();

            var client = (INginxHubClient)host.Services.GetService(typeof(INginxHubClient));
            client.Connect("nginxHub");

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
    }
}
