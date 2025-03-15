using System.Linq;
using Microsoft.Extensions.Logging;

namespace Candy.AspNet
{
    public static class LoggingExtensions
    {
        private static readonly string[] candyName = new[] { "Candy" };

        public static ILoggingBuilder AddCandyLogging(
            this ILoggingBuilder logging,
            params string[] highlightWords
        )
        {
            logging.ClearProviders();
            logging.AddConsole(o => o.FormatterName = "CustomFormatter");
            logging.AddConsoleFormatter<CustomConsoleFormatter, CustomConsoleFormatterOptions>(
                options =>
                {
                    options.HighlightWords = highlightWords.Concat(candyName);
                }
            );
            logging.InitializeCandyDebugClass();

            return logging;
        }
    }
}
