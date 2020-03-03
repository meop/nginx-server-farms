using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace NginxServerFarms.Services {
    public class NginxConfigService : INginxConfigService {
        private static readonly MemoryCache MemoryCache =
            new MemoryCache(new MemoryCacheOptions());

        private FileSystemWatcher fileSystemWatcher;

        private string configPath;
        private int fileWatchDebounceTimeMs;

        public void Watch(
            string configPath,
            int fileWatchDebounceTimeMs) {
            this.configPath = configPath;
            this.fileWatchDebounceTimeMs = fileWatchDebounceTimeMs;

            var directory = Path.GetDirectoryName(configPath);
            var file = Path.GetFileName(configPath);

            fileSystemWatcher = new FileSystemWatcher(directory, file) {
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
                    configPath,
                    configPath,
                    cacheEntryOptions);
            }

            fileSystemWatcher.Changed += (obj, e) => DelayedRestart();
            fileSystemWatcher.Created += (obj, e) => DelayedRestart();
            fileSystemWatcher.Deleted += (obj, e) => DelayedRestart();
            fileSystemWatcher.Disposed += (obj, e) => DelayedRestart();
            fileSystemWatcher.Error += (obj, e) => DelayedRestart();
            fileSystemWatcher.Renamed += (obj, e) => DelayedRestart();

            fileSystemWatcher.EnableRaisingEvents = true;
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

            this.ReadConfig();
        }

        private static readonly string Offline = "#";
        private static readonly string Begin = "{";
        private static readonly string End = "}";
        private static readonly string Upstream = "upstream";
        private static readonly string OnlineServer = "server";
        private static readonly string OfflineServer = $"{Offline} {OnlineServer}";

        private static bool IsUpstream(string line) =>
            line.Trim()
                .StartsWith(Upstream);
        private static bool IsServer(string line) {
            var trimmed = line.Trim();
            return trimmed.StartsWith(OnlineServer) ||
                   trimmed.StartsWith(OfflineServer);
        }
        private static bool IsEnabled(string line) =>
            !line.Trim()
                 .StartsWith(Offline);
        private static bool IsEnd(string line) =>
            line.Trim()
                .StartsWith(End);

        private static string GetUpstreamName(string line) =>
            line.Replace(Upstream, string.Empty)
                .Replace(Begin, string.Empty)
                .Trim();

        private static string EnableServer(string line) =>
            line.Replace(OfflineServer, OnlineServer);
        private static string DisableServer(string line) =>
            line.Replace(OnlineServer, OfflineServer);

        public async Task WriteConfig(IReadOnlyList<NginxUpstream> upstreams) {
            fileSystemWatcher.EnableRaisingEvents = false;

            var lines = new List<string>();
            string line;
            using (var file = new StreamReader(this.configPath)) {
                IReadOnlyList<NginxUpstreamServer> upstreamServers = null;
                while ((line = file.ReadLine()) != null) {
                    if (upstreamServers != null) {
                        if (IsEnd(line)) {
                            upstreamServers = null;
                        } else if(IsServer(line)) {
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

            File.WriteAllLines(this.configPath, lines);

            await Task.Delay(this.fileWatchDebounceTimeMs).ConfigureAwait(false);
            fileSystemWatcher.EnableRaisingEvents = true;

            this.OnRaiseConfigWriteEvent(new NginxConfigChangedArgs
            {
                Upstreams = upstreams
            });
        }

        private IReadOnlyList<NginxUpstream> ReadConfig() {
            var upstreams = new List<NginxUpstream>();

            string line;
            using (var file = new StreamReader(this.configPath)) {
                NginxUpstream upstream = null;
                List<NginxUpstreamServer> upstreamServers = null;
                while ((line = file.ReadLine()) != null) {
                    if (upstream != null) {
                        if (IsEnd(line)) {
                            upstream.Servers = upstreamServers;
                            upstreams.Add(upstream);

                            upstream = null;
                            upstreamServers = null;
                        } else if(IsServer(line)) {
                            var enabled = IsEnabled(line);
                            var server = new NginxUpstreamServer {
                                Enabled = enabled,
                                Entry = line.Trim()
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
            }

            this.OnRaiseConfigReadEvent(new NginxConfigChangedArgs
            {
                Upstreams = upstreams
            });

            return upstreams;
        }

        public event EventHandler<NginxConfigChangedArgs> ConfigReadEvent;
        public event EventHandler<NginxConfigChangedArgs> ConfigWriteEvent;

        protected virtual void OnRaiseConfigReadEvent(NginxConfigChangedArgs e) {
            ConfigReadEvent?.Invoke(this, e);
        }

        protected virtual void OnRaiseConfigWriteEvent(NginxConfigChangedArgs e)
        {
            ConfigWriteEvent?.Invoke(this, e);
        }
    }
}