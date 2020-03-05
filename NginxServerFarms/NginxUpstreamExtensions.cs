using System.Collections.Generic;
using System.Linq;

namespace NginxServerFarms {
    internal static class NginxUpstreamExtensions {

        private static readonly string Offline = "#";
        private static readonly string Begin = "{";
        private static readonly string End = "}";
        private static readonly string Upstream = "upstream";
        private static readonly string OnlineServer = "server";
        private static readonly string OfflineServer = $"{Offline} {OnlineServer}";

        public static bool IsUpstream(string line) =>
                    line.Trim()
                        .StartsWith(Upstream);

        public static bool IsServer(string line) {
            var trimmed = line.Trim();
            return trimmed.StartsWith(OnlineServer) ||
                   trimmed.StartsWith(OfflineServer);
        }

        public static bool IsEnabled(string line) =>
            !line.Trim()
                 .StartsWith(Offline);

        public static bool IsEnd(string line) =>
            line.Trim()
                .StartsWith(End);

        public static string GetUpstreamName(string line) =>
            line.Replace(Upstream, string.Empty)
                .Replace(Begin, string.Empty)
                .Trim();

        public static string EnableServer(string line) =>
            line.Replace(OfflineServer, OnlineServer);

        public static string DisableServer(string line) =>
            line.Replace(OnlineServer, OfflineServer);

        public static void SafeMerge(
            this IReadOnlyList<NginxUpstream> upstreams,
            IReadOnlyList<NginxUpstream> updatedUpstreams
        ) {
            var upstreamsList = upstreams.ToArray();
            foreach (var upstream in upstreamsList) {
                foreach (var updatedUpstream in updatedUpstreams) {
                    if (upstream.Name == updatedUpstream.Name) {
                        var upstreamServersList = upstream.Servers.ToArray();
                        foreach (var server in upstreamServersList) {
                            foreach (var updatedServer in updatedUpstream.Servers) {
                                if (server.Entry == updatedServer.Entry) {
                                    server.Enabled = updatedServer.Enabled;
                                }
                            }
                        }
                        upstream.Servers = upstreamServersList;
                    }
                }
            }
            upstreams = upstreamsList;
        }
    }
}