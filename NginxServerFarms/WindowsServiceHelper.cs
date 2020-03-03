using System.Diagnostics;
using System.ServiceProcess;

namespace NginxServerFarms {
    internal static class WindowsServiceHelper {
        private static readonly object ServiceLock = new object();

        public static void ForceRestart(
            string serviceName,
            string processName) {
            lock (ServiceLock) {
                var sc = new ServiceController(serviceName);
                if (sc.Status != ServiceControllerStatus.Stopped) {
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                }

                var processes = Process.GetProcessesByName(processName);
                if (processes.Length > 0) {
                    foreach (var process in processes) {
                        process.Kill();
                    }
                }

                sc.Start();
                // not waiting this time, because it might fail, etc
            }
        }
    }
}
