using System.Collections.Generic;
using System.Linq;

namespace NginxServerFarms {
    internal static class NginxUpstreamExtensions {

        private static readonly char Space = ' ';
        private static readonly char Offline = '#';
        private static readonly char Begin = '{';
        private static readonly char End = '}';
        private static readonly string Upstream = "upstream ";
        private static readonly string OnlineServer = "server ";
        private static readonly string OfflineServer = "# server ";

        public static bool IsUpstream(
            string line
        ) =>
            line.TrimStart()
                .StartsWith(Upstream);

        public static bool IsServer(
            string line
        ) =>
            line.TrimStart()
                .TrimStart(Offline)
                .TrimStart()
                .StartsWith(OnlineServer);

        public static bool IsEnabled(
            string line
        ) =>
            !line.TrimStart()
                 .StartsWith(Offline);

        public static bool IsEnd(
            string line
        ) =>
            line.TrimStart()
                .StartsWith(End);

        public static string GetUpstreamName(
            string line
        ) =>
            line.Replace(Upstream, string.Empty)
                .Replace(Begin, Space)
                .Trim();

        public static string GetServerEntry(
            string line
        ) =>
            EnableServer(line).Trim();

        public static string EnableServer(
            string line
        ) {
            var indexOffline = line.IndexOf(Offline);
            var indexServer = line.IndexOf(OnlineServer);
            return indexOffline > -1
                ? line.Remove(indexOffline, indexServer - indexOffline)
                : line;
        }

        public static string DisableServer(
            string line
        ) =>
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