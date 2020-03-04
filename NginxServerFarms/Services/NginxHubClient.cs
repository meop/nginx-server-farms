using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;

namespace NginxServerFarms.Services {
    internal class NginxHubClient : INginxHubClient {
        private readonly INginxConfigFileService configFileService;
        private HubConnection hubConnection;

        public NginxHubClient(
            INginxConfigFileService configFileService) {
            this.configFileService = configFileService;
        }

        public async Task Connect(string hubPath) {
            // todo mporter: figure out how to inject this base url
            this.hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:5001/nginxHub")
                .WithAutomaticReconnect()
                .Build();

            await this.hubConnection.StartAsync().ConfigureAwait(false);

            this.configFileService.UpstreamsChangedEvent += (s, e) =>
                Task.WaitAll(this.hubConnection.InvokeAsync("RefreshConfigs", e.Upstreams));
        }
    }
}