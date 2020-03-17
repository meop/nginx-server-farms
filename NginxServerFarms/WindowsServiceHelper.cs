using System.ServiceProcess;

using static NginxServerFarms.WindowsHelper;

namespace NginxServerFarms {
    internal static class WindowsServiceHelper {
        private static readonly object Mutex = new object();

        public static void Restart(
            string serviceName,
            string processFileName
        ) {
            lock (Mutex) {
                var sc = new ServiceController(serviceName);
                if (sc.Status != ServiceControllerStatus.Stopped) {
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                }

                ForceKill(processFileName);

                sc.Start();
                // not waiting this time, because it might fail, etc
            }
        }
    }
}
