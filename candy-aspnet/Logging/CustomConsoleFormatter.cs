using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace Candy.AspNet
{
    /// <summary>
    /// A custom console formatter that provides colorized, structured logging output.
    /// Features include:
    /// - Colored output based on log level
    /// - Timestamp prefix
    /// - Shortened category names to maintain readable line lengths
    /// - Exception formatting with stack trace highlighting
    /// - Word highlighting in stack traces
    /// </summary>
    public class CustomConsoleFormatter : ConsoleFormatter
    {
        private const string EndColor = "\u001b[0m";  // ANSI escape code to reset text color

        private readonly CustomConsoleFormatterOptions _options;

        public CustomConsoleFormatter(IOptions<CustomConsoleFormatterOptions> options)
            : base("CustomFormatter")
        {
            _options = options.Value;
        }

        /// <summary>
        /// Main entry point for formatting and writing a log entry.
        /// Format: [Time] [LogLevel] [Category][EventId] Message
        /// Example: 12:34:56 ERROR MyApp.Service[123] An error occurred
        /// </summary>
        public override void Write<TState>(
            in LogEntry<TState> logEntry,
            IExternalScopeProvider scopeProvider,
            TextWriter textWriter
        )
        {
            var logLevel = logEntry.LogLevel;
            var logLevelColor = GetLogLevelColor(logLevel);

            // Build the log line components
            var timeStr = GetTimeString();
            var logLevelStr = GetLogLevelString(logLevel, logLevelColor);
            var contextStr = GetContextString(logEntry);
            var messageStr = GetMessageString(logEntry);

            // Write the main log message with proper alignment
            Console.Write($"{timeStr} {logLevelStr} {contextStr}");
            var totalLength = timeStr.Length + logLevelStr.Length + contextStr.Length;
            var padding = new string(' ', Math.Max(0, totalLength - 15));

            // Handle multi-line messages with proper indentation
            var lines = messageStr.Split("\n");
            Console.WriteLine($" {logLevelColor}{lines[0]}{EndColor}");
            foreach (var line in lines.Skip(1))
            {
                Console.WriteLine($"{padding}{logLevelColor}{line}{EndColor}");
            }

            // Format exception details if present
            if (logEntry.Exception != null)
            {
                var exceptionType = logEntry.Exception.GetType().FullName ?? "Unknown";
                var exceptionMessage = logEntry.Exception.Message ?? "Empty";

                Console.WriteLine($" \n  Exception: {logLevelColor}{exceptionType}{EndColor}");
                Console.WriteLine($"  Message: {logLevelColor}{exceptionMessage}{EndColor}\n ");

                if (logEntry.Exception.StackTrace != null)
                {
                    var stackTraceLines = logEntry.Exception.StackTrace.Split('\n');
                    for (int i = 0; i < stackTraceLines.Length; i++)
                    {
                        var line = stackTraceLines[i]
                            .Trim()
                            .Replace(Environment.CurrentDirectory, "");
                        if (line.Contains("---"))
                        {
                            // Gray color for stack trace separators
                            Console.WriteLine($"  \u001b[90m{line}{EndColor}");
                        }
                        else
                        {
                            // Highlight important words in stack traces
                            var highlightedLine = HighlightWords(line, _options.HighlightWords);
                            Console.WriteLine($"  {highlightedLine}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the ANSI color code for each log level:
        /// - Trace: Gray
        /// - Debug: Cyan
        /// - Info: Default color
        /// - Warning: Yellow
        /// - Error: Red
        /// - Critical: Magenta
        /// </summary>
        private string GetLogLevelColor(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => "\u001b[90m", // Gray
                LogLevel.Debug => "\u001b[36m", // Cyan
                LogLevel.Information => "",
                LogLevel.Warning => "\u001b[33m", // Yellow
                LogLevel.Error => "\u001b[31m", // Red
                LogLevel.Critical => "\u001b[35m", // Magenta
                _ => "",
            };
        }

        /// <summary>
        /// Returns the current time formatted as HH:mm:ss
        /// </summary>
        private string GetTimeString()
        {
            return $"{DateTime.Now:HH:mm:ss}";
        }

        /// <summary>
        /// Formats the log level as a 5-character string with color
        /// Example: "ERROR", "WARN ", "INFO ", etc.
        /// </summary>
        private string GetLogLevelString(LogLevel logLevel, string logLevelColor)
        {
            var logLevelStr = logLevel switch
            {
                LogLevel.Trace => "TRACE",
                LogLevel.Debug => "DEBUG",
                LogLevel.Information => "INFO",
                LogLevel.Warning => "WARN",
                LogLevel.Error => "ERROR",
                LogLevel.Critical => "FATAL",
                _ => logLevel.ToString().ToUpper(),
            };
            return $"{logLevelColor}{logLevelStr.PadLeft(5)}{EndColor}";
        }

        /// <summary>
        /// Formats the context (category and event ID) with a fixed width of 30 characters.
        /// The category name is shortened if necessary to maintain the fixed width.
        /// Example: "MyApp.Service[123]" -> "MyApp.Ser[123]"
        /// </summary>
        private string GetContextString<TState>(in LogEntry<TState> logEntry)
        {
            const int limit = 30;  // Fixed width for the context section
            var eventId = $"[{logEntry.EventId.Id}]";
            var categoryLimit = limit - eventId.Length;
            var category = ShortenCategory(logEntry.Category, categoryLimit);
            var context = $"{category}{eventId}";
            return $"\u001b[90m{context.PadRight(limit)}{EndColor}";
        }

        /// <summary>
        /// Intelligently shortens a category name to fit within a specified length limit.
        /// Handles several cases:
        /// 1. Removes compiler-generated async method names
        /// 2. For two-part names (e.g. "Namespace.Class"), preserves the class name
        /// 3. For longer names, gradually shortens the longest parts while maintaining readability
        /// 4. Ensures no negative lengths are used in string operations
        /// </summary>
        private string ShortenCategory(string category, int limit)
        {
            // Handle special case of async methods by removing compiler-generated parts
            if (category.Contains("+<"))
            {
                category = category.Split('+')[0];
            }

            if (limit <= 0)
                return "";

            if (category.Length <= limit)
                return category;

            var parts = category.Split('.');

            if (parts.Length == 2)
            {
                // For simple "Namespace.Class" format, prioritize keeping the class name
                var lastLength = parts[1].Length;
                var remainingLength = Math.Max(0, limit - lastLength - 1);
                var first = parts[0][..Math.Min(parts[0].Length, remainingLength)];
                var last = parts[1];
                var result = $"{first}.{last}";
                return result.Length <= limit ? result : result[..limit];
            }
            if (parts.Length > 2)
            {
                // For longer paths, gradually shorten the longest parts
                var last = parts.Last();
                var remainingLength = Math.Max(0, limit - last.Length - 1); // -1 for the dot
                var firstParts = parts.Take(parts.Length - 1).ToList();

                while (firstParts.Count > 1)
                {
                    var totalLength = firstParts.Sum(p => p.Length) + firstParts.Count - 1;
                    if (totalLength <= remainingLength)
                    {
                        var abbreviated = string.Join(".", firstParts);
                        return $"{abbreviated}.{last}";
                    }

                    // Find and shorten the longest part
                    var longestPart = firstParts.OrderByDescending(p => p.Length).First();
                    var index = firstParts.IndexOf(longestPart);
                    if (longestPart.Length > 2)
                    {
                        firstParts[index] = longestPart[..(longestPart.Length - 1)];
                    }
                    else
                    {
                        firstParts.RemoveAt(index);
                    }
                }

                if (firstParts.Count == 0)
                    return last.Length <= limit ? last : last[..limit];

                var first = firstParts.First();
                var firstLength = Math.Min(first.Length, Math.Max(0, remainingLength));
                return $"{first[..firstLength]}.{last}";
            }

            return category[..Math.Min(category.Length, limit)];
        }

        /// <summary>
        /// Formats the log message using the provided formatter
        /// </summary>
        private string GetMessageString<TState>(in LogEntry<TState> logEntry)
        {
            return logEntry.Formatter(logEntry.State, logEntry.Exception);
        }

        /// <summary>
        /// Highlights specified words in a line of text using cyan color.
        /// Used primarily for emphasizing important parts in stack traces.
        /// </summary>
        private string HighlightWords(string line, IEnumerable<string> wordsToHighlight)
        {
            foreach (var word in wordsToHighlight)
            {
                line = line.Replace(word, $"\u001b[36m{word}\u001b[0m\u001b[37m");
            }
            return line;
        }
    }

    /// <summary>
    /// Configuration options for the CustomConsoleFormatter.
    /// Allows specifying words to highlight in stack traces.
    /// </summary>
    public class CustomConsoleFormatterOptions : ConsoleFormatterOptions
    {
        public IEnumerable<string> HighlightWords { get; set; } = new List<string>();
    }
}
