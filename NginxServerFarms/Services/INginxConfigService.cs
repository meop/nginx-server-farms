using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NginxServerFarms.Services {
    public interface INginxConfigService {
        void Watch(
            string configPath,
            int fileWatchDebounceTimeMs);

        Task WriteConfig(IReadOnlyList<NginxUpstream> upstreams);

        event EventHandler<NginxConfigChangedArgs> ConfigReadEvent;
        event EventHandler<NginxConfigChangedArgs> ConfigWriteEvent;
    }
}