using Lemon.HandyLib.Logging.Enrichers;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Globalization;

namespace Lemon.HandyLib.Logging.Definitions
{
    public class LogEntry
    {
        private static DateTime lastLogTime;
        private static int? processId;
        public LogEntry(
            Guid id,
            DateTime timestamp,
            string level,
            int processId,
            int threadId,
            string intervalGraph,
            string interval,
            string caller,
            string message,
            string? exception = null,
            LogEntryType type = LogEntryType.Log)
        {
            Id = id;
            Timestamp = timestamp;
            Level = level;
            ProcessId = processId;
            ThreadId = threadId;
            IntervalGraph = intervalGraph;
            Interval = interval;
            Caller = caller;
            Message = message;
            Exception = exception;
            Type = type;
        }
        public Guid Id { get; private set; }
        public DateTime Timestamp { get; private set; }
        public string Level { get; private set; }
        public int ProcessId { get; private set; }
        public int ThreadId { get; private set; }
        public string IntervalGraph { get; private set; }
        public string Interval { get; private set; }
        public string Caller { get; private set; }
        public string Message { get; private set; }
        public string? Exception { get; private set; }
        public LogEntryType Type { get; private set; } = LogEntryType.Log;

        private static int CurrentProcessId
        {
            get
            {
#if NET6_0_OR_GREATER
                return Environment.ProcessId;
#else
                if (!processId.HasValue)
                {
                    using var process = Process.GetCurrentProcess();
                    processId = process.Id;
                }
                return processId.Value;
#endif
            }
        }

        
        public static LogEntry ParseLog(string logLine,
            char splitChar = '|',
            DateTime? timeStamp = null,  
            int? threadId = null,  
            LogEntryType logEntryType = LogEntryType.Log)
        {
            try
            {
                if (string.IsNullOrEmpty(logLine))
                {
                    logLine = "null";
                }
                if (!threadId.HasValue)
                {
                    threadId = Environment.CurrentManagedThreadId;
                }
                if (!timeStamp.HasValue)
                {
                    timeStamp = DateTime.Now;
                }

                var interval = timeStamp.Value - lastLogTime;
                var intervalTuple = TimeIntervalEnricher.GenerateGraph(interval);

                if (logEntryType == LogEntryType.ConsoleIn)
                {
                    return new LogEntry(
                        id: Guid.NewGuid(),
                        timestamp: timeStamp.Value,
                        level: LogLevel.Information.ToShortString(),
                        processId: CurrentProcessId,
                        threadId: threadId.Value,
                        intervalGraph: intervalTuple.Graph,
                        interval: intervalTuple.Interval,
                        caller: nameof(Console),
                        message: logLine,
                        exception: null,
                        type: LogEntryType.ConsoleIn
                    );
                }
                if (logEntryType == LogEntryType.Log)
                {
                    return new LogEntry(
                                id: Guid.NewGuid(),
                                timestamp: timeStamp.Value,
                                level: LogLevel.Information.ToShortString(),
                                processId: CurrentProcessId,
                                threadId: threadId.Value,
                                intervalGraph: intervalTuple.Graph,
                                interval: intervalTuple.Interval,
                                caller: nameof(Console),
                                message: logLine,
                                exception: null,
                                type: LogEntryType.ConsoleOut);
                }
                var parts = logLine.Split(splitChar);

                if (parts.Length < 8)
                {
                    return new LogEntry(
                        id: Guid.NewGuid(),
                        timestamp: timeStamp.Value,
                        level: LogLevel.Information.ToShortString(),
                        processId: CurrentProcessId,
                        threadId: threadId.Value,
                        intervalGraph: intervalTuple.Graph,
                        interval: intervalTuple.Interval,
                        caller: nameof(Console),
                        message: logLine,
                        exception: null,
                        type: LogEntryType.ConsoleOut
                    );
                }
                else
                {
                    return new LogEntry(
                        id: Guid.NewGuid(),
                        timestamp: DateTime.ParseExact(parts[0].Trim(), "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                        level: parts[1].Trim(),
                        processId: int.Parse(parts[2].Trim()),
                        threadId: int.Parse(parts[3].Trim()),
                        intervalGraph: parts[4].Trim(),
                        interval: parts[5].Trim(),
                        caller: parts[6].Trim(),
                        message: parts[7].Trim(),
                        exception: parts.Length > 8 ? parts[8].Trim() : null,
                        type: logEntryType
                    );
                }
            }
            finally
            {
                lastLogTime = DateTime.Now;
            }
        }
    }
    public enum LogEntryType
    {
        ConsoleOut,
        ConsoleIn,
        Log,
    }
}