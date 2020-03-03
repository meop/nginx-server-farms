using System.Collections.Generic;

namespace NginxServerFarms {
    public class NginxUpstream {
        public string Name { get; set; }
        public IReadOnlyList<NginxUpstreamServer> Servers { get; set; }
    }
}
