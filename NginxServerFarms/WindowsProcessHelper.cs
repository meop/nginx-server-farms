using System.Diagnostics;
using System.IO;

namespace NginxServerFarms {
    internal static class WindowsProcessHelper {
        private static readonly object Mutex = new object();

        private static Process P;

        private static void SafeStart(
            string processFileDir,
            string processFileName
        ) {
            var processPath = Path.Join(processFileDir, processFileName);

            P = new Process {
                StartInfo = new ProcessStartInfo(processPath) {
                    WorkingDirectory = processFileDir
                },
            };

            P.Start();
        }

        private static void SafeStop() {
            if (P != null) {
                P.Kill(true);
                P.WaitForExit();
            }
        }

        public static void Start(
            string processFileDir,
            string processFileName
        ) {
            lock (Mutex) {
                SafeStart(processFileDir, processFileName);
            }
        }

        public static void Stop() {
            lock (Mutex) {
                SafeStop();
            }
        }

        public static void Restart(
            string processFileDir,
            string processFileName
        ) {
            lock (Mutex) {
                SafeStop();

                SafeStart(processFileDir, processFileName);
            }
        }
    }
}
