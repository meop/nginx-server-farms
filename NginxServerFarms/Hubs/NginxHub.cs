using Microsoft.AspNetCore.SignalR;
using NginxServerFarms.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NginxServerFarms.Hubs {
    public class NginxHub : Hub {
        private readonly INginxConfigService configService;

        public NginxHub(
            INginxConfigService configService) {
            this.configService = configService;
            this.configService.ConfigReadEvent += HandleConfigReadEvent;
        }

        public void HandleConfigReadEvent(object sender, NginxConfigChangedArgs e) {
            Clients.All.SendAsync("RefreshConfigs", e.Upstreams);
        }

        public async Task SaveConfig(NginxUpstream upstream) =>
            await this.SaveConfigs(new[] { upstream }).ConfigureAwait(false);

        public async Task SaveConfigs(IReadOnlyList<NginxUpstream> upstreams) {
            await this.configService.WriteConfig(upstreams).ConfigureAwait(false);
        }
    }
}