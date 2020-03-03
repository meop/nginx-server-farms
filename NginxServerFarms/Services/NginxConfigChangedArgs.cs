using System;
using System.Collections.Generic;

namespace NginxServerFarms {
    public class NginxConfigChangedArgs : EventArgs {
        public IReadOnlyList<NginxUpstream> Upstreams { get; set; }
    }
}