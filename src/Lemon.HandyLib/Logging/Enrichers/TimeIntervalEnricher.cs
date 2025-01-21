using Serilog.Core;
using Serilog.Events;
using System;
using System.Linq;

namespace Lemon.HandyLib.Logging.Enrichers
{
    public class TimeIntervalEnricher : ILogEventEnricher
    {
        private static DateTime? _lastLogTime;

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var currentTime = logEvent.Timestamp.UtcDateTime;

            if (_lastLogTime.HasValue)
            {
                var interval = currentTime - _lastLogTime.Value;
                var result = GenerateGraph(interval);
                logEvent.AddPropertyIfAbsent(new LogEventProperty("IntervalGraph", new ScalarValue(result.Graph)));
                logEvent.AddPropertyIfAbsent(new LogEventProperty("Interval", new ScalarValue(result.Interval)));
            }

            _lastLogTime = currentTime;
        }

        public static (string Graph, string Interval) GenerateGraph(TimeSpan timeSpan, int unitMs = 100)
        {
            char[] template = Enumerable.Range(0, 9).Select(_ => ' ').ToArray();
            try
            {
                var timeSeconds = timeSpan.TotalMilliseconds / unitMs;

                for (int i = 0; i < timeSeconds; i++)
                {
                    if (i > template.Length * 2 - 1)
                    {
                        break;
                    }
                    if (i > template.Length - 1)
                    {
                        template[i - template.Length] = '=';
                        continue;
                    }
                    template[i] = '-';
                }
                return ($"[{new string(template)}]", timeSpan.TotalMilliseconds.ToString("0.0"));
            }
            catch
            {
                return ($"[##########]", timeSpan.TotalMilliseconds.ToString("0.0"));
            }
        }
    }
}
