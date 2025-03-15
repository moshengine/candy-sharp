using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Candy.AspNet
{
    public static class Debug
    {
        private static ILoggerFactory _loggerFactory;

        public static void InitializeCandyDebugClass(this ILoggingBuilder loggingBuilder)
        {
            _loggerFactory = loggingBuilder
                .Services.BuildServiceProvider()
                .GetRequiredService<ILoggerFactory>();
        }

        public static void Log(string message)
        {
            var callerType = new StackTrace().GetFrame(1)?.GetMethod()?.DeclaringType;
            var callerTypeFullName = callerType?.FullName ?? "Unknown";
            LogWithLevel(LogLevel.Information, callerTypeFullName, message);
        }

        public static void LogWarning(string message)
        {
            var callerType = new StackTrace().GetFrame(1)?.GetMethod()?.DeclaringType;
            var callerTypeFullName = callerType?.FullName ?? "Unknown";
            LogWithLevel(LogLevel.Warning, callerTypeFullName, message);
        }

        public static void LogError(string message)
        {
            var callerType = new StackTrace().GetFrame(1)?.GetMethod()?.DeclaringType;
            var callerTypeFullName = callerType?.FullName ?? "Unknown";
            LogWithLevel(LogLevel.Error, callerTypeFullName, message);
        }

        public static void LogException(Exception exception)
        {
            var callerType = new StackTrace().GetFrame(1)?.GetMethod()?.DeclaringType;
            var callerTypeFullName = callerType?.FullName ?? "Unknown";
            LogWithLevel(LogLevel.Error, callerTypeFullName, exception.ToString());
        }

        public static void Assert(bool condition, string message = "Assertion failed")
        {
            if (!condition)
            {
                var callerType = new StackTrace().GetFrame(1)?.GetMethod()?.DeclaringType;
                var callerTypeFullName = callerType?.FullName ?? "Unknown";
                LogWithLevel(LogLevel.Error, callerTypeFullName, $"Assert: {message}");
                throw new AssertionException(message);
            }
        }

        public static void LogTrace(string message)
        {
            var callerType = new StackTrace().GetFrame(1)?.GetMethod()?.DeclaringType;
            var callerTypeFullName = callerType?.FullName ?? "Unknown";
            LogWithLevel(LogLevel.Trace, callerTypeFullName, message);
        }

        public static void LogDebug(string message)
        {
            var callerType = new StackTrace().GetFrame(1)?.GetMethod()?.DeclaringType;
            var callerTypeFullName = callerType?.FullName ?? "Unknown";
            LogWithLevel(LogLevel.Debug, callerTypeFullName, message);
        }

        public static void LogCritical(string message)
        {
            var callerType = new StackTrace().GetFrame(1)?.GetMethod()?.DeclaringType;
            var callerTypeFullName = callerType?.FullName ?? "Unknown";
            LogWithLevel(LogLevel.Critical, callerTypeFullName, message);
        }

        private static void LogWithLevel(
            LogLevel logLevel,
            string callerTypeFullName,
            string message
        )
        {
            if (_loggerFactory == null)
            {
                throw new Exception("Debug not initialized");
            }

            var logger = _loggerFactory.CreateLogger(callerTypeFullName);
            logger.Log(logLevel, message);
        }
    }

    public class AssertionException : Exception
    {
        public AssertionException(string message)
            : base(message) { }
    }
}
