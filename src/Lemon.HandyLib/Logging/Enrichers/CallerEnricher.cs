using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;
using System.Linq;

namespace Lemon.HandyLib.Logging.Enrichers
{
    class CallerEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var skip = 7;
            var trace = new StackTrace(true);
            var stacks = trace.GetFrames();
            if (skip >= stacks.Length)
            {
                skip = stacks.Length - 1;
            }
            while (true)
            {
                var stack = stacks[skip];
                if (!stack.HasMethod())
                {
                    logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller", new ScalarValue("<unknown method>")));
                    return;
                }

                var method = stack.GetMethod();
                if (method!.DeclaringType!.Assembly != typeof(Log).Assembly)
                {
                    if (logEvent.Level > LogEventLevel.Debug)
                    {
                        var caller = $"{method.DeclaringType.FullName}.{method.Name}({string.Join(", ", method.GetParameters().Select(pi => pi.ParameterType.Name))}).{stack.GetFileLineNumber()}";
                        logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller", new ScalarValue(caller)));
                    }
                    else
                    {
                        var caller = $"{method.DeclaringType.FullName}";
                        logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller", new ScalarValue(caller)));
                    }

                    return;
                }

                skip++;
            }
        }
    }

}
