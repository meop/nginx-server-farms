using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace NginxServerFarms.Services {
    internal class NginxHubClient : INginxHubClient {
        private readonly IConfiguration configuration;
        private readonly INginxConfigFileService configFileService;
        private HubConnection hubConnection;

        public NginxHubClient(
            IConfiguration configuration,
            INginxConfigFileService configFileService) {
            this.configuration = configuration;
            this.configFileService = configFileService;
        }

        public async Task Connect(string hubPath) {
            var baseUrl = this.configuration
                .GetSection("Kestrel")
                .GetSection("EndPoints")
                .GetSection("Https")
                .GetValue<string>("Url");
            this.hubConnection = new HubConnectionBuilder()
                .WithUrl($"{baseUrl}/nginxHub")
                .WithAutomaticReconnect()
                .Build();

            await this.hubConnection.StartAsync().ConfigureAwait(false);

            this.configFileService.UpstreamsChangedEvent += (s, e) =>
                Task.WaitAll(this.hubConnection.InvokeAsync("RefreshConfigs", e.Upstreams));
        }
    }
}