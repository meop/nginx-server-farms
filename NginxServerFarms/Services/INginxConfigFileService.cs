﻿using System;
using System.Collections.Generic;

namespace NginxServerFarms.Services {
    public interface INginxConfigFileService {
        void Watch(
            string filePath,
            int fileWatchDebounceTimeMs);

        IReadOnlyList<NginxUpstream> ReadUpstreams();
        void WriteUpstreams(IReadOnlyList<NginxUpstream> upstreams);

        event EventHandler<NginxConfigChangedArgs> UpstreamsChangedEvent;
    }
}
