using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Candy.Logging
{
    /// <summary>
    /// Extension methods for configuring logging with Candy's custom formatting.
    /// Uses Serilog for industry-standard, high-performance logging with beautiful console output.
    /// </summary>
    public static class LoggingExtensions
    {
        private static readonly string[] candyName = new[] { "Candy" };

        /// <summary>
        /// Adds Serilog logging with Candy's custom formatting to the host builder.
        /// This provides beautiful, Spring Boot-inspired console output with:
        /// - Fixed-width alignment so all messages start at the same horizontal position
        /// - Intelligent category name shortening
        /// - Color-coded log levels
        /// - Enhanced exception formatting with stack trace highlighting
        /// </summary>
        /// <param name="hostBuilder">The host builder to configure</param>
        /// <param name="highlightWords">Words to highlight in stack traces (e.g. your project name)</param>
        /// <returns>The host builder for method chaining</returns>
        public static IHostBuilder UseCandyLogging(
            this IHostBuilder hostBuilder,
            params string[] highlightWords
        ) => UseCandyLogging(hostBuilder, null, highlightWords);

        /// <summary>
        /// Adds Serilog logging with Candy's custom formatting to the host builder
        /// with additional Serilog configuration.
        /// Use this overload when you need more control over Serilog configuration
        /// (e.g., adding file sinks, cloud sinks, enrichers, filters, etc.).
        /// </summary>
        /// <param name="hostBuilder">The host builder to configure</param>
        /// <param name="configureLogger">Action to configure the Serilog LoggerConfiguration</param>
        /// <param name="highlightWords">Words to highlight in stack traces</param>
        /// <returns>The host builder for method chaining</returns>
        public static IHostBuilder UseCandyLogging(
            this IHostBuilder hostBuilder,
            System.Action<LoggerConfiguration> configureLogger,
            params string[] highlightWords
        )
        {
            return hostBuilder.ConfigureLogging((context, logging) =>
            {
                // Clear default providers to prevent duplicate logs!
                logging.ClearProviders();

                var allHighlightWords = highlightWords.Concat(candyName).ToArray();

                var loggerConfig = new LoggerConfiguration()
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(new CandySerilogFormatter(allHighlightWords, AnsiConsoleTheme.Code));

                configureLogger?.Invoke(loggerConfig);

                logging.AddSerilog(loggerConfig.CreateLogger(), dispose: true);
                logging.InitializeCandyDebugClass();
            });
        }
    }
}
