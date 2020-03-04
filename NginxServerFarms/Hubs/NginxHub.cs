using Microsoft.AspNetCore.SignalR;
using NginxServerFarms.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NginxServerFarms.Hubs {
    internal class NginxHub : Hub {
        private readonly INginxConfigFileService configFileService;

        public NginxHub(
            INginxConfigFileService configFileService) {
            this.configFileService = configFileService;
        }

        public Task RefreshConfigs(IReadOnlyList<NginxUpstream> upstreams) {
            return Clients.All.SendAsync("RefreshConfigs", upstreams);
        }

        public Task<IReadOnlyList<NginxUpstream>> GetUpstreams() {
            return Task.FromResult(this.configFileService.ReadUpstreams());
        }

        public Task SaveUpstream(NginxUpstream upstream) {
            return this.SaveUpstreams(new[] { upstream });
        }

        public Task SaveUpstreams(IReadOnlyList<NginxUpstream> upstreams) {
            this.configFileService.WriteUpstreams(upstreams);
            return Task.CompletedTask;
        }
    }
}