using Serilog;
using System;
using System.IO;

namespace Lemon.HandyLib.Logging
{
    public static class SerilLogHelper
    {
        private static ILogger? _logger;
        public static ILogger Logger
        {
            get
            {
                if (_logger == null)
                {
                    throw new InvalidOperationException($"Please ensure calling {nameof(Config)} in advance!");
                }
                return _logger;
            }
        }
        public static void Config(string productName)
        {
            if (_logger == null)
            {
                const string LogTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} | {Level:u3} | {ProcessId} | {ThreadId:0000} | {IntervalGraph} | {Interval} | {Caller} | {Message:lj}{NewLine}{Exception}";

                var rootDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var remainFileCount = 10;
                var remainFileDay = TimeSpan.FromDays(3);

                var configuration = new LoggerConfiguration()
                                      .MinimumLevel.Debug()
                                      .Enrich.FromLogContext()
                                      .Enrich.WithProcessId()
                                      .Enrich.WithThreadId()
                                      .Enrich.WithCaller()
                                      .Enrich.WithInterval()
                                      //.Enrich.With<InvocationContextEnricher>()
                                      .WriteTo.Console(outputTemplate: LogTemplate)
                                      .WriteTo.Map(
                                        e => DateTime.Now.ToString("yyyy-MM-dd"),
                                        (t, wt) => wt.File(
                                            Path.Combine(rootDir, $"{productName}-logs", t, $"{productName}.txt"),
                                            shared: true,
                                            rollOnFileSizeLimit: true,
                                            retainedFileCountLimit: remainFileCount,
                                            retainedFileTimeLimit: remainFileDay,
                                            outputTemplate: LogTemplate,
                                            fileSizeLimitBytes: 10 * 1024 * 1024,
                                            flushToDiskInterval: TimeSpan.FromSeconds(15)
                                      ));
                _logger = configuration.CreateLogger();
                Log.Logger = _logger;
                Information($"Config client to {productName}");
            }
            else
            {
                throw new InvalidOperationException("Serilog has been configured!");
            }
        }


        public static void Information(string message)
        {
            Logger.Information(message);
        }

        public static void Warning(string message)
        {
            Logger.Warning(message);
        }

        public static void Warning(Exception exception, string message)
        {
            Logger.Warning(exception, message);
        }

        public static void LogWarning(Exception exception, string message)
        {
            Logger.Warning(exception, message);
        }

        public static void Error(string message)
        {
            Logger.Error(message);
        }

        public static void Error(Exception exception,string message)
        {
            Logger.Error(exception,message);
        }
        public static void Error(Exception exception)
        {
            Logger.Error(exception,"");
        }
        public static void Debug(string message)
        {
            Logger.Debug(message);
        }

        public static void Fatal(string message)
        {
            Logger.Fatal(message);
        }
    }

}
