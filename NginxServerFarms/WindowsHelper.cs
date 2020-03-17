using System.Diagnostics;
using System.IO;

namespace NginxServerFarms {
    internal static class WindowsHelper {
        public static void ForceKill(
            string processFileName
        ) {
            var processes = Process.GetProcessesByName(
                Path.GetFileNameWithoutExtension(processFileName));
            if (processes.Length > 0) {
                foreach (var process in processes) {
                    process.Kill(true);
                    process.WaitForExit();
                }
            }
        }
    }
}
