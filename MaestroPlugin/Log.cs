using System;
using System.IO;
using System.Reflection;

namespace MaestroPlugin
{
    internal class Log
    {
        private static string LogPath => $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\Logs\";

        public static void Start()
        {
            try
            {
                if (Directory.Exists(LogPath)) Directory.Delete(LogPath, true);

                Directory.CreateDirectory(LogPath);
            }
            catch { }
        }

        public static void This(string message, string callsign = null)
        {
            try
            {
                string logFile = string.Empty;

                if (callsign == null)
                    logFile = Path.Combine(LogPath, "Messages.txt");
                else
                    logFile = Path.Combine(LogPath, $"{callsign}.json");

                if (!File.Exists(logFile)) File.Create(logFile);

                using (var file = new StreamWriter(logFile, callsign == null))
                {
                    if (callsign == null) message = $"{DateTime.UtcNow}:{message}";
                    file.WriteLine(message);
                }
            }
            catch { }
        }
    }
}
