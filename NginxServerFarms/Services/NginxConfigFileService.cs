using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

using static NginxServerFarms.NginxUpstreamExtensions;

namespace NginxServerFarms.Services {
    internal class NginxConfigFileService : INginxConfigFileService {
        private static readonly MemoryCache MemoryCache =
            new MemoryCache(new MemoryCacheOptions());

        private readonly object configFilePathLock = new object();
        private string configFilePath;

        private IReadOnlyList<NginxUpstream> upstreams;

        private FileSystemWatcher fileSystemWatcher;
        private int fileWatchDebounceTimeMs;

        public void Watch(
            string configFilePath,
            int fileWatchDebounceTimeMs) {
            this.configFilePath = configFilePath;
            this.fileWatchDebounceTimeMs = fileWatchDebounceTimeMs;

            var directory = Path.GetDirectoryName(configFilePath);
            var file = Path.GetFileName(configFilePath);

            this.fileSystemWatcher = new FileSystemWatcher(directory, file) {
                IncludeSubdirectories = true,
                NotifyFilter =
                    NotifyFilters.Attributes |
                    NotifyFilters.CreationTime |
                    NotifyFilters.DirectoryName |
                    NotifyFilters.FileName |
                    NotifyFilters.LastWrite
            };

            // form of debouncing for filesystem changes
            void DelayedRestart() {
                var expirationTime = TimeSpan.FromMilliseconds(fileWatchDebounceTimeMs);
                var expirationDateTime = DateTime.Now.Add(expirationTime);
                var expirationToken = new CancellationChangeToken(
                    new CancellationTokenSource(expirationTime).Token);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetPriority(CacheItemPriority.NeverRemove)
                    .SetAbsoluteExpiration(expirationTime)
                    .AddExpirationToken(expirationToken)
                    .RegisterPostEvictionCallback(FileAltered);

                MemoryCache.Set(
                    configFilePath,
                    configFilePath,
                    cacheEntryOptions);
            }

            this.fileSystemWatcher.Changed += (obj, e) => DelayedRestart();
            this.fileSystemWatcher.Created += (obj, e) => DelayedRestart();
            this.fileSystemWatcher.Deleted += (obj, e) => DelayedRestart();
            this.fileSystemWatcher.Disposed += (obj, e) => DelayedRestart();
            this.fileSystemWatcher.Error += (obj, e) => DelayedRestart();
            this.fileSystemWatcher.Renamed += (obj, e) => DelayedRestart();

            this.fileSystemWatcher.EnableRaisingEvents = true;
        }

        private void FileAltered(
            object key,
            object value,
            EvictionReason reason,
            object state
        ) {
            // toss out earlier duplicates
            if (reason != EvictionReason.TokenExpired) {
                return;
            }

            this.LoadUpstreams();
            this.OnRaiseUpstreamsChangedEvent(new NginxConfigChangedArgs {
                Upstreams = this.upstreams
            });
        }

        public IReadOnlyList<NginxUpstream> ReadUpstreams() {
            if (this.upstreams == null) {
                this.LoadUpstreams();
            }
            return this.upstreams;
        }

        public void LoadUpstreams() {
            lock (this.configFilePathLock) {
                var upstreams = new List<NginxUpstream>();

                string line;
                using var file = new StreamReader(this.configFilePath);

                NginxUpstream upstream = null;
                List<NginxUpstreamServer> upstreamServers = null;
                while ((line = file.ReadLine()) != null) {
                    if (upstream != null) {
                        if (IsEnd(line)) {
                            upstream.Servers = upstreamServers;
                            upstreams.Add(upstream);

                            upstream = null;
                            upstreamServers = null;
                        } else if (IsServer(line)) {
                            var enabled = IsEnabled(line);
                            var server = new NginxUpstreamServer {
                                Enabled = enabled,
                                Entry = EnableServer(line.Trim())
                            };
                            if (upstreamServers == null) {
                                upstreamServers = new List<NginxUpstreamServer> {
                                        server
                                    };
                            } else {
                                upstreamServers.Add(server);
                            }
                        }
                    }

                    if (IsUpstream(line)) {
                        upstream = new NginxUpstream {
                            Name = GetUpstreamName(line)
                        };
                    }
                }

                this.upstreams = upstreams;
            }
        }

        public void WriteUpstreams(IReadOnlyList<NginxUpstream> upstreams) {
            this.fileSystemWatcher.EnableRaisingEvents = false;
            this.SaveUpstreams(upstreams);

            this.OnRaiseUpstreamsChangedEvent(new NginxConfigChangedArgs {
                Upstreams = this.upstreams
            });

            // small delay to wait for any lingering fs events from the write above
            Thread.Sleep(this.fileWatchDebounceTimeMs);
            this.fileSystemWatcher.EnableRaisingEvents = true;
        }

        private void SaveUpstreams(IReadOnlyList<NginxUpstream> upstreams) {
            lock (this.configFilePathLock) {
                var lines = new List<string>();
                string line;
                using (var file = new StreamReader(this.configFilePath)) {
                    IReadOnlyList<NginxUpstreamServer> upstreamServers = null;
                    while ((line = file.ReadLine()) != null) {
                        if (upstreamServers != null) {
                            if (IsEnd(line)) {
                                upstreamServers = null;
                            } else if (IsServer(line)) {
                                foreach (var upstreamServer in upstreamServers) {
                                    if (line.Contains(upstreamServer.Entry)) {
                                        line = upstreamServer.Enabled
                                            ? EnableServer(line)
                                            : DisableServer(line);
                                        break;
                                    }
                                }
                            }
                        }
                        if (IsUpstream(line)) {
                            foreach (var upstream in upstreams) {
                                if (line.Contains(upstream.Name)) {
                                    upstreamServers = upstream.Servers;
                                    break;
                                }
                            }
                        }
                        lines.Add(line);
                    }
                }

                File.WriteAllLines(this.configFilePath, lines);

                this.upstreams.SafeMerge(upstreams);
            }
        }

        public event EventHandler<NginxConfigChangedArgs> UpstreamsChangedEvent;

        protected virtual void OnRaiseUpstreamsChangedEvent(NginxConfigChangedArgs e) {
            UpstreamsChangedEvent?.Invoke(this, e);
        }
    }
}