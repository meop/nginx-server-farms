using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NginxServerFarms.Hubs {
    internal class NginxHub : Hub {
        public Task RefreshConfigs(IReadOnlyList<NginxUpstream> upstreams) {
            return Clients.All.SendAsync("RefreshConfigs", upstreams);
        }
    }
}