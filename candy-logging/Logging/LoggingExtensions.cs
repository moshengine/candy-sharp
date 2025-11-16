using System.Linq;
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

        #region IHostApplicationBuilder extensions (for .NET 6+ projects)

        /// <summary>
        /// Adds Serilog logging with Candy's custom formatting to the host application builder.
        /// For use with Host.CreateApplicationBuilder() in .NET 6+ projects.
        /// This provides beautiful, Spring Boot-inspired console output with:
        /// - Fixed-width alignment so all messages start at the same horizontal position
        /// - Intelligent category name shortening
        /// - Color-coded log levels
        /// - Enhanced exception formatting with stack trace highlighting
        /// </summary>
        /// <param name="builder">The host application builder to configure</param>
        /// <param name="highlightWords">Words to highlight in stack traces (e.g. your project name)</param>
        /// <returns>The host application builder for method chaining</returns>
        public static IHostApplicationBuilder UseCandyLogging(
            this IHostApplicationBuilder builder,
            params string[] highlightWords
        ) => UseCandyLogging(builder, null!, highlightWords);

        /// <summary>
        /// Adds Serilog logging with Candy's custom formatting to the host application builder
        /// with additional Serilog configuration.
        /// For use with Host.CreateApplicationBuilder() in .NET 6+ projects.
        /// Use this overload when you need more control over Serilog configuration
        /// (e.g., adding file sinks, cloud sinks, enrichers, filters, etc.).
        /// </summary>
        /// <param name="builder">The host application builder to configure</param>
        /// <param name="configureLogger">Action to configure the Serilog LoggerConfiguration</param>
        /// <param name="highlightWords">Words to highlight in stack traces</param>
        /// <returns>The host application builder for method chaining</returns>
        public static IHostApplicationBuilder UseCandyLogging(
            this IHostApplicationBuilder builder,
            System.Action<LoggerConfiguration>? configureLogger,
            params string[] highlightWords
        )
        {
            // Clear default providers to prevent duplicate logs!
            builder.Logging.ClearProviders();

            var allHighlightWords = highlightWords.Concat(candyName).ToArray();

            var loggerConfig = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console(new CandySerilogFormatter(allHighlightWords, AnsiConsoleTheme.Code));

            configureLogger?.Invoke(loggerConfig);

            builder.Logging.AddSerilog(loggerConfig.CreateLogger(), dispose: true);
            builder.Logging.InitializeCandyDebugClass();

            return builder;
        }

        #endregion

        #region IHostBuilder extensions (deprecated - use IHostApplicationBuilder instead)

        /// <summary>
        /// Adds Serilog logging with Candy's custom formatting to the host builder.
        /// For use with traditional IHostBuilder pattern.
        /// This provides beautiful, Spring Boot-inspired console output with:
        /// - Fixed-width alignment so all messages start at the same horizontal position
        /// - Intelligent category name shortening
        /// - Color-coded log levels
        /// - Enhanced exception formatting with stack trace highlighting
        /// </summary>
        /// <param name="hostBuilder">The host builder to configure</param>
        /// <param name="highlightWords">Words to highlight in stack traces (e.g. your project name)</param>
        /// <returns>The host builder for method chaining</returns>
        [System.Obsolete("Use the IHostApplicationBuilder overload with Host.CreateApplicationBuilder() instead. This method is retained for backward compatibility.")]
        public static IHostBuilder UseCandyLogging(
            this IHostBuilder hostBuilder,
            params string[] highlightWords
        ) => UseCandyLogging(hostBuilder, null!, highlightWords);

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
        [System.Obsolete("Use the IHostApplicationBuilder overload with Host.CreateApplicationBuilder() instead. This method is retained for backward compatibility.")]
        public static IHostBuilder UseCandyLogging(
            this IHostBuilder hostBuilder,
            System.Action<LoggerConfiguration>? configureLogger,
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

        #endregion
    }
}
