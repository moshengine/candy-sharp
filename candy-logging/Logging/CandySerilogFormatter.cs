using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using Serilog.Sinks.SystemConsole.Themes;

namespace MoshEngine.Candy.Logging
{
    /// <summary>
    /// A custom Serilog formatter that provides colorized, structured logging output.
    /// Features include:
    /// - Colored output based on log level
    /// - Timestamp prefix
    /// - Shortened category names to maintain readable line lengths
    /// - Exception formatting with stack trace highlighting
    /// - Word highlighting in stack traces
    /// - Fixed horizontal position for messages (alignment)
    /// </summary>
    public class CandySerilogFormatter : ITextFormatter
    {
        private const string EndColor = "\u001b[0m";  // ANSI escape code to reset text color
        private const string CyanColor = "\u001b[36m"; // Cyan color for messages and context (Serilog-style)
        private const string GrayColor = "\u001b[90m"; // Gray color for timestamps and metadata

        private readonly IEnumerable<string> _highlightWords;
        private readonly ConsoleTheme _theme;
        private readonly ValueFormatterDelegate _valueFormatter;

        public CandySerilogFormatter(IEnumerable<string>? highlightWords = null, ConsoleTheme? theme = null)
        {
            _highlightWords = highlightWords ?? new List<string>();
            _theme = theme ?? AnsiConsoleTheme.Code;
            
            // Create a value formatter that uses the theme for rendering
            _valueFormatter = (logEvent, name, value, format, output) =>
            {
                RenderThemedValue(value, output, format);
            };
        }

        private void RenderThemedValue(LogEventPropertyValue value, TextWriter output, string? format)
        {
            if (value is ScalarValue sv)
            {
                RenderScalarValue(sv, output, format);
            }
            else if (value is SequenceValue seq)
            {
                RenderSequence(seq, output);
            }
            else if (value is StructureValue str)
            {
                RenderStructure(str, output);
            }
            else if (value is DictionaryValue dict)
            {
                RenderDictionary(dict, output);
            }
            else
            {
                output.Write(value.ToString());
            }
        }

        private void RenderScalarValue(ScalarValue scalar, TextWriter output, string? format)
        {
            if (scalar.Value == null)
            {
                ApplyStyle(output, ConsoleThemeStyle.Null, "null");
                return;
            }

            if (scalar.Value is string)
            {
                ApplyStyle(output, ConsoleThemeStyle.String, scalar.Value.ToString()!);
            }
            else if (scalar.Value is bool)
            {
                ApplyStyle(output, ConsoleThemeStyle.Boolean, scalar.Value.ToString()!.ToLowerInvariant());
            }
            else if (IsNumeric(scalar.Value))
            {
                var formatted = TryFormatValue(scalar.Value, format);
                ApplyStyle(output, ConsoleThemeStyle.Number, formatted);
            }
            else if (scalar.Value is DateTime dt)
            {
                var formatted = TryFormatValue(dt, format);
                ApplyStyle(output, ConsoleThemeStyle.Scalar, formatted);
            }
            else if (scalar.Value is DateTimeOffset dto)
            {
                var formatted = TryFormatValue(dto, format);
                ApplyStyle(output, ConsoleThemeStyle.Scalar, formatted);
            }
            else if (scalar.Value is TimeSpan ts)
            {
                var formatted = TryFormatValue(ts, format);
                ApplyStyle(output, ConsoleThemeStyle.Scalar, formatted);
            }
            else
            {
                var formatted = TryFormatValue(scalar.Value, format);
                ApplyStyle(output, ConsoleThemeStyle.Scalar, formatted);
            }
        }

        /// <summary>
        /// Safely tries to format a value with the given format string.
        /// Falls back to default ToString() if formatting fails.
        /// </summary>
        private string TryFormatValue(object value, string? format)
        {
            if (string.IsNullOrEmpty(format))
            {
                return value?.ToString() ?? "null";
            }

            try
            {
                if (value is IFormattable formattable)
                {
                    return formattable.ToString(format, null);
                }
                
                return string.Format("{0:" + format + "}", value);
            }
            catch (FormatException)
            {
                // If the format string is invalid, fall back to default formatting
                return value?.ToString() ?? "null";
            }
        }

        private void RenderSequence(SequenceValue sequence, TextWriter output)
        {
            ApplyStyle(output, ConsoleThemeStyle.TertiaryText, "[");
            var first = true;
            foreach (var element in sequence.Elements)
            {
                if (!first) ApplyStyle(output, ConsoleThemeStyle.TertiaryText, ", ");
                RenderThemedValue(element, output, null);
                first = false;
            }
            ApplyStyle(output, ConsoleThemeStyle.TertiaryText, "]");
        }

        private void RenderStructure(StructureValue structure, TextWriter output)
        {
            ApplyStyle(output, ConsoleThemeStyle.TertiaryText, "{");
            var first = true;
            foreach (var prop in structure.Properties)
            {
                if (!first) ApplyStyle(output, ConsoleThemeStyle.TertiaryText, ", ");
                ApplyStyle(output, ConsoleThemeStyle.Name, prop.Name);
                ApplyStyle(output, ConsoleThemeStyle.TertiaryText, ": ");
                RenderThemedValue(prop.Value, output, null);
                first = false;
            }
            ApplyStyle(output, ConsoleThemeStyle.TertiaryText, "}");
        }

        private void RenderDictionary(DictionaryValue dictionary, TextWriter output)
        {
            ApplyStyle(output, ConsoleThemeStyle.TertiaryText, "{");
            var first = true;
            foreach (var element in dictionary.Elements)
            {
                if (!first) ApplyStyle(output, ConsoleThemeStyle.TertiaryText, ", ");
                ApplyStyle(output, ConsoleThemeStyle.TertiaryText, "[");
                RenderThemedValue(element.Key, output, null);
                ApplyStyle(output, ConsoleThemeStyle.TertiaryText, "]: ");
                RenderThemedValue(element.Value, output, null);
                first = false;
            }
            ApplyStyle(output, ConsoleThemeStyle.TertiaryText, "}");
        }

        private void ApplyStyle(TextWriter output, ConsoleThemeStyle style, string text)
        {
            var colorCode = _theme.Set(output, style);
            output.Write(text);
            if (colorCode > 0)
            {
                _theme.Reset(output);
            }
        }

        private bool IsNumeric(object value)
        {
            return value is int || value is long || value is float || value is double || 
                   value is decimal || value is short || value is byte || value is uint ||
                   value is ulong || value is ushort || value is sbyte;
        }

        private delegate void ValueFormatterDelegate(LogEvent logEvent, string name, LogEventPropertyValue value, string? format, TextWriter output);

        /// <summary>
        /// Main entry point for formatting and writing a log entry.
        /// Format: [Time] [LogLevel] [Category][EventId] Message
        /// Example: 12:34:56 ERROR MyApp.Service[123] An error occurred
        /// </summary>
        public void Format(LogEvent logEvent, TextWriter output)
        {
            var logLevel = ConvertLogLevel(logEvent.Level);
            var logLevelColor = GetLogLevelColor(logLevel);

            // Build the log line components
            var timeStr = GetTimeString(logEvent);
            var logLevelStr = GetLogLevelString(logLevel, logLevelColor);
            var contextStr = GetContextString(logEvent);

            // Write the main log message with proper alignment
            output.Write($"{timeStr} {logLevelStr} {contextStr} ");

            // Render the message using Serilog's themed renderer for syntax highlighting
            RenderMessageTemplate(logEvent, output);

            // Format exception details if present
            if (logEvent.Exception != null)
            {
                var exceptionType = logEvent.Exception.GetType().FullName ?? "Unknown";
                var exceptionMessage = logEvent.Exception.Message ?? "Empty";

                output.WriteLine($" \n  {GrayColor}Exception:{EndColor} {CyanColor}{exceptionType}{EndColor}");
                output.WriteLine($"  {GrayColor}Message:{EndColor} {CyanColor}{exceptionMessage}{EndColor}\n ");

                if (logEvent.Exception.StackTrace != null)
                {
                    var stackTraceLines = logEvent.Exception.StackTrace.Split('\n');
                    for (int i = 0; i < stackTraceLines.Length; i++)
                    {
                        var line = stackTraceLines[i]
                            .Trim()
                            .Replace(Environment.CurrentDirectory, "");
                        if (line.Contains("---"))
                        {
                            // Gray color for stack trace separators
                            output.WriteLine($"  \u001b[90m{line}{EndColor}");
                        }
                        else
                        {
                            // Highlight important words in stack traces
                            var highlightedLine = HighlightWords(line, _highlightWords);
                            output.WriteLine($"  {GrayColor}{highlightedLine}{EndColor}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Converts Serilog's LogEventLevel to Microsoft.Extensions.Logging.LogLevel
        /// </summary>
        private string ConvertLogLevel(LogEventLevel level)
        {
            return level switch
            {
                LogEventLevel.Verbose => "Trace",
                LogEventLevel.Debug => "Debug",
                LogEventLevel.Information => "Information",
                LogEventLevel.Warning => "Warning",
                LogEventLevel.Error => "Error",
                LogEventLevel.Fatal => "Critical",
                _ => "Information"
            };
        }

        /// <summary>
        /// Returns the ANSI color code for each log level:
        /// - Trace: Gray
        /// - Debug: Gray
        /// - Info: Default color
        /// - Warning: Yellow
        /// - Error: Red
        /// - Critical: Magenta
        /// </summary>
        private string GetLogLevelColor(string logLevel)
        {
            return logLevel switch
            {
                "Trace" => "\u001b[90m", // Gray
                "Debug" => "\u001b[90m", // Gray
                "Information" => "",
                "Warning" => "\u001b[33m", // Yellow
                "Error" => "\u001b[31m", // Red
                "Critical" => "\u001b[35m", // Magenta
                _ => "",
            };
        }

        /// <summary>
        /// Returns the timestamp formatted as HH:mm:ss with gray color
        /// </summary>
        private string GetTimeString(LogEvent logEvent)
        {
            return $"{GrayColor}{logEvent.Timestamp:HH:mm:ss}{EndColor}";
        }

        /// <summary>
        /// Formats the log level as a 3-character string with color
        /// Example: "ERR", "WRN", "INF", etc.
        /// </summary>
        private string GetLogLevelString(string logLevel, string logLevelColor)
        {
            var logLevelStr = logLevel switch
            {
                "Trace" => "TRC",
                "Debug" => "DBG",
                "Information" => "INF",
                "Warning" => "WRN",
                "Error" => "ERR",
                "Critical" => "CRT",
                _ => logLevel.ToUpper(),
            };
            return $"{logLevelColor}{logLevelStr}{EndColor}";
        }

        /// <summary>
        /// Formats the context (source context and event ID) with a fixed width of 30 characters.
        /// The category name is shortened if necessary to maintain the fixed width.
        /// Example: "MyApp.Service[123]" -> "MyApp.Ser[123]"
        /// </summary>
        private string GetContextString(LogEvent logEvent)
        {
            const int limit = 30;  // Fixed width for the context section
            
            // Extract SourceContext property (analogous to category in Microsoft.Extensions.Logging)
            var sourceContext = "Unknown";
            if (logEvent.Properties.TryGetValue("SourceContext", out var sourceContextValue))
            {
                sourceContext = sourceContextValue.ToString().Trim('"');
            }

            // Extract EventId if present
            var eventIdStr = "";
            if (logEvent.Properties.TryGetValue("EventId", out var eventIdValue))
            {
                // Handle structured EventId (e.g., { Id: 100, Name: "RequestStart" })
                if (eventIdValue is StructureValue structuredEventId)
                {
                    // Try to extract just the Id from the structured value
                    var idProp = structuredEventId.Properties.FirstOrDefault(p => p.Name == "Id");
                    
                    if (idProp != null && idProp.Value is ScalarValue idScalar)
                    {
                        eventIdStr = $"[{idScalar.Value}]";
                    }
                }
                else if (eventIdValue is ScalarValue scalarEventId)
                {
                    // For simple scalar EventIds, use them directly
                    eventIdStr = $"[{scalarEventId.Value}]";
                }
            }

            var categoryLimit = limit - eventIdStr.Length;
            var category = ShortenCategory(sourceContext, categoryLimit);
            var context = $"{category}{eventIdStr}";
            return $"{GrayColor}{context.PadRight(limit)}{EndColor}";
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

        private void RenderMessageTemplate(LogEvent logEvent, TextWriter output)
        {
            var messageTemplate = logEvent.MessageTemplate;
            var properties = logEvent.Properties;
            
            var tokenIndex = 0;
            foreach (var token in messageTemplate.Tokens)
            {
                if (token is Serilog.Parsing.TextToken textToken)
                {
                    output.Write(textToken.Text);
                }
                else if (token is Serilog.Parsing.PropertyToken propertyToken)
                {
                    if (properties.TryGetValue(propertyToken.PropertyName, out var propertyValue))
                    {
                        RenderThemedValue(propertyValue, output, propertyToken.Format);
                    }
                    else
                    {
                        output.Write(propertyToken.ToString());
                    }
                }
                tokenIndex++;
            }
            output.WriteLine();
        }

        /// <summary>
        /// Highlights specified words in a line of text using bright cyan color.
        /// Used primarily for emphasizing important parts in stack traces.
        /// </summary>
        private string HighlightWords(string line, IEnumerable<string> wordsToHighlight)
        {
            foreach (var word in wordsToHighlight)
            {
                line = line.Replace(word, $"{EndColor}\u001b[96m{word}{EndColor}{GrayColor}");
            }
            return line;
        }
    }
}
