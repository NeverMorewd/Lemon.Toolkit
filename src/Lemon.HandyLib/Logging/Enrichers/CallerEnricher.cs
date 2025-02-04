using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using System.Diagnostics;
using System.Linq;

namespace Lemon.HandyLib.Logging.Enrichers
{
    class CallerEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var trace = new StackTrace(true);
            for (int i = 1; i < trace.FrameCount; i++)
            {
                var frame = trace.GetFrame(i);
                var method = frame.GetMethod();
                if (method.DeclaringType != typeof(Logger) 
                    && method.DeclaringType != typeof(LoggerConfiguration)
                    && method.DeclaringType != typeof(SerilLogHelper)
                     && method!.DeclaringType!.Assembly != typeof(SerilogLoggerFactory).Assembly
                     && method!.DeclaringType!.Assembly != typeof(Microsoft.Extensions.Logging.ILogger).Assembly
                     && method!.DeclaringType!.Assembly != typeof(Microsoft.Extensions.Logging.LoggerFactory).Assembly
                    && method!.DeclaringType!.Assembly != typeof(Log).Assembly)
                {
                    if (logEvent.Level > LogEventLevel.Debug)
                    {
                        var caller = $"{method.DeclaringType.FullName}.{method.Name}({string.Join(", ", method.GetParameters().Select(pi => pi.ParameterType.Name))}).{frame.GetFileLineNumber()}";
                        logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller", new ScalarValue(caller)));
                    }
                    else
                    {
                        var caller = $"{method.DeclaringType.FullName}";
                        logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller", new ScalarValue(caller)));
                    }
                    break;
                }
            }
            //if (skip >= stacks.Length)
            //{
            //    skip = stacks.Length - 1;
            //}
            //while (true)
            //{
            //    var stack = stacks[skip];
            //    if (!stack.HasMethod())
            //    {
            //        logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller", new ScalarValue("<unknown method>")));
            //        return;
            //    }

            //    var method = stack.GetMethod();
            //    if (method!.DeclaringType!.Assembly != typeof(Log).Assembly)
            //    {
            //        if (logEvent.Level > LogEventLevel.Debug)
            //        {
            //            var caller = $"{method.DeclaringType.FullName}.{method.Name}({string.Join(", ", method.GetParameters().Select(pi => pi.ParameterType.Name))}).{stack.GetFileLineNumber()}";
            //            logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller", new ScalarValue(caller)));
            //        }
            //        else
            //        {
            //            var caller = $"{method.DeclaringType.FullName}";
            //            logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller", new ScalarValue(caller)));
            //        }

            //        return;
            //    }

            //    skip++;
            //}
        }
    }

}
