using System;
using System.Globalization;

namespace Lemon.HandyLib.Logging.Definitions
{
    public class LogEntry
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }
        public int ProcessId { get; set; }
        public int ThreadId { get; set; }
        public string Interval { get; set; }
        public string Caller { get; set; }
        public string Message { get; set; }
        public string? Exception { get; set; }

        public static LogEntry ParseLog(string logLine)
        {
            if (string.IsNullOrEmpty(logLine))
                throw new ArgumentException("Log line cannot be null or empty.");

            var parts = logLine.Split('|');

            if (parts.Length < 7)
            {
                return new LogEntry
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.Now,
                    Level = "Trace",
                    ProcessId = -1,
                    ThreadId = Environment.CurrentManagedThreadId,
                    Interval = "",
                    Caller = "",
                    Message = logLine,
                    Exception = null
                };
            }
            else
            {
                return new LogEntry
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.ParseExact(parts[0].Trim(), "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                    Level = parts[1].Trim(),
                    ProcessId = int.Parse(parts[2].Trim()),
                    ThreadId = int.Parse(parts[3].Trim()),
                    Interval = parts[4].Trim(),
                    Caller = parts[5].Trim(),
                    Message = parts[6].Trim(),
                    Exception = parts.Length > 7 ? parts[7].Trim() : null // 异常信息是可选的
                };
            }
        }
    }
}
