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
            // 当前日志的时间
            var currentTime = logEvent.Timestamp.UtcDateTime;

            // 计算与上一条日志的时间间隔
            if (_lastLogTime.HasValue)
            {
                var interval = currentTime - _lastLogTime.Value;
                logEvent.AddPropertyIfAbsent(new LogEventProperty("Interval", new ScalarValue(Caculate(interval))));
            }

            // 更新最后一次记录的时间
            _lastLogTime = currentTime;
        }

        private string Caculate(TimeSpan timeSpan, int unitMs = 100)
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
                return $"[{new string(template)}]:{(int)timeSpan.TotalMilliseconds}ms";
            }
            catch
            {
                return $"[Too long]";
            }
        }
    }
}
