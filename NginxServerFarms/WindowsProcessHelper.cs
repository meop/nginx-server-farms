using System.Diagnostics;
using System.IO;

using static NginxServerFarms.WindowsHelper;

namespace NginxServerFarms {
    internal static class WindowsProcessHelper {
        private static readonly object Mutex = new object();

        private static Process P;

        public static void Restart(
            string processFileDir,
            string processFileName
        ) {
            lock (Mutex) {
                if (P != null) {
                    P.Kill(true);
                    P.WaitForExit();
                }

                ForceKill(processFileName);

                var processPath = Path.Join(processFileDir, processFileName);

                P = new Process {
                    StartInfo = new ProcessStartInfo(processPath) {
                        WorkingDirectory = processFileDir
                    },
                };

                P.Start();
                // not waiting this time, because it might fail, etc
            }
        }
    }
}
