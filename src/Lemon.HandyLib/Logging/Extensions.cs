using Lemon.HandyLib.Logging.Definitions;
using Microsoft.Extensions.Logging;
using Serilog.Configuration;
using Serilog;
using System.Runtime.CompilerServices;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using System;
using System.Collections.Generic;
using Lemon.HandyLib.Logging.Enrichers;

namespace Lemon.HandyLib.Logging
{
    public static class Extensions
    {
        public static void LogDebug(
           this ILogger logger,
           Exception ex,
           string? message = null,
           [CallerFilePath] string filePath = "",
           params object[] parameters)
        {
            logger.LogDebug(EventIds.DebugDefault, ex, message.NotNullOr(ex.Message), parameters);
        }

        public static void LogInformation(
            this ILogger logger,
            Exception ex,
            string? message = null,
            [CallerFilePath] string filePath = "",
            params object[] parameters)
        {
            logger.LogInformation(EventIds.InfoDefault, ex, message.NotNullOr(ex.Message), parameters);
        }

        public static void LogWarning(
           this ILogger logger,
           Exception ex,
           string? message = null,
           [CallerFilePath] string filePath = "",
           params object[] parameters)
        {
            logger.LogWarning(EventIds.WarningDefault, ex, message.NotNullOr(ex.Message), parameters);
        }

        public static void LogError(
           this ILogger logger,
           Exception ex,
           string? message = null,
           [CallerFilePath] string filePath = "",
           params object[] parameters)
        {
            logger.LogError(EventIds.ErrorDefault, ex, message.NotNullOr(ex.Message), parameters);
        }

        public static void LogCritical(
           this ILogger logger,
           Exception ex,
           string? message = null,
           [CallerFilePath] string filePath = "",
           params object[] parameters)
        {
            logger.LogCritical(EventIds.ErrorDefault, ex, message.NotNullOr(ex.Message), parameters);
        }

        public static void WithStreamScope(
            this ILogger logger,
            Action<ILogger> logAction,
            string? mode = null,
            string? stage = null,
            string? action = null,
            string? requestId = null)
        {
            using var scope = logger.BeginScope(
                new Dictionary<string, object>
                {
                    [StreamScopeDefs.ScopeNameKey] = "Stream",
                    [StreamScopeDefs.IsAsyncKey] = mode ?? StreamScopeDefs.UnknownValue,
                    [StreamScopeDefs.ClientKey] = stage ?? StreamScopeDefs.UnknownValue,
                    [StreamScopeDefs.ActionKey] = action ?? StreamScopeDefs.UnknownValue,
                    [StreamScopeDefs.RequestIdKey] = requestId ?? StreamScopeDefs.UnknownValue,
                });
            logAction(logger);
        }

        public static void WithStreamScope(
            this ILogger logger,
            Action<ILogger> logAction,
            StreamScope streamScope)
        {
            logger.WithStreamScope(
                logAction,
                streamScope.IsAsync,
                streamScope.Client,
                streamScope.Action,
                streamScope.RequestId);
        }

        public static void WithContext(
            this ILogger logger,
            Action<ILogger> loggerAction,
            [CallerMemberName] string? callerMemberName = null,
            [CallerFilePath] string? callerFilePath = null,
            [CallerLineNumber] int callerLineNumber = 0)
        {
            using var scope = logger.BeginScope(new
            {
                callerMemberName,
                callerFilePath,
                callerLineNumber
            });
            loggerAction(logger);
        }

        public static string? NotNullOr(
            this string? source,
            string replace,
            bool replaceEmpty = true,
            bool replaceWhiteSpace = false)
        {
            bool shouldReplace = source == null;

            shouldReplace = shouldReplace || (string.IsNullOrEmpty(source) && replaceEmpty);
            shouldReplace = shouldReplace || (string.IsNullOrWhiteSpace(source) && replaceWhiteSpace);

            if (shouldReplace)
            {
                return replace;
            }

            return source;
        }

        public static string ToShortString(this LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => "TRA",
                LogLevel.Debug => "DBG",
                LogLevel.Information => "INF",
                LogLevel.Warning => "WAR",
                LogLevel.Error => "ERR",
                LogLevel.Critical => "CRI",
                LogLevel.None => "NON",
                _ => "DBG",
            };
        }
        public static KeyValuePair<TKey, TValue> Tag<TKey, TValue>(this TValue value, TKey key)
        {
            return new KeyValuePair<TKey, TValue>(key, value);
        }

        public static LoggerConfiguration WithCaller(this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            return enrichmentConfiguration.With<CallerEnricher>();
        }
        public static LoggerConfiguration WithInterval(this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            return enrichmentConfiguration.With<TimeIntervalEnricher>();
        }
    }
}
