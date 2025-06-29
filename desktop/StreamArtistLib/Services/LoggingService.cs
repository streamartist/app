using System;
using System.Diagnostics;
using System.IO;

namespace StreamArtist.Services
{
    public class LoggingService
    {
        private static LoggingService _instance;
        private static readonly object _lock = new object();
        private readonly string _logDirectory;

        private LoggingService(string logDirectory = "Logs")
        {
            string binaryPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string binaryDir = System.IO.Path.GetDirectoryName(binaryPath);
            _logDirectory = Path.Combine(binaryDir, "logs");
            Directory.CreateDirectory(_logDirectory);
        }

        public string LogDirectory { get { return _logDirectory; } }

        public static LoggingService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new LoggingService();
                        }
                    }
                }
                return _instance;
            }
        }

        public void Log(string message)
        {
            lock (_lock)
            {
                try
                {
                    Debug.WriteLine(message);
                    string fileName = $"{DateTime.Now:yyyy-MM-dd}.log";
                    string fullPath = Path.Combine(_logDirectory, fileName);

                    string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";

                    File.AppendAllText(fullPath, logEntry + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    // Avoid crashing the app if logging fails.
                    Debug.WriteLine($"Failed to write to log file: {ex.Message}");
                }
            }
        }
    }
}
