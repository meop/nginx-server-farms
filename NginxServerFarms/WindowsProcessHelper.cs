using System.Diagnostics;
using System.IO;

namespace NginxServerFarms {
    internal static class WindowsProcessHelper {
        private static readonly object Mutex = new object();

        private static Process P;

        public static void ForceRestart(
            string processDir,
            string processName) {
            lock (Mutex) {
                if (P != null) {
                    P.Kill(true);
                    P.WaitForExit();
                }

                var processPath = Path.Join(processDir, processName);

                P = new Process {
                    StartInfo = new ProcessStartInfo(processPath) {
                        WorkingDirectory = processDir
                    },
                };

                P.Start();
                // not waiting this time, because it might fail, etc
            }
        }
    }
}
