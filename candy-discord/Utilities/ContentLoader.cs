using Microsoft.Extensions.Logging;

namespace Candy.Discord;

public class ContentLoader
{
    private readonly ILogger<ContentLoader> _logger;

    public ContentLoader(ILogger<ContentLoader> logger)
    {
        _logger = logger;
    }

    public async Task<string?> LoadContentAsync(string basePath, string fileName)
    {
        try
        {
            var filePath = Path.Combine(basePath, fileName);

            if (!File.Exists(filePath))
            {
                _logger.LogError("Content file not found: {FilePath}", filePath);
                return null;
            }

            var content = await File.ReadAllTextAsync(filePath);
            _logger.LogDebug("Loaded content from {FilePath}", filePath);
            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading content from {BasePath}/{FileName}", basePath, fileName);
            return null;
        }
    }

    public async Task<string?> LoadContentWithFallbackAsync(string relativeFilePath)
    {
        var sourceContentPath = Path.Combine(Directory.GetCurrentDirectory(), relativeFilePath);
        var outputContentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativeFilePath);

        if (File.Exists(sourceContentPath))
        {
            return await LoadContentFromPathAsync(sourceContentPath);
        }

        if (File.Exists(outputContentPath))
        {
            return await LoadContentFromPathAsync(outputContentPath);
        }

        _logger.LogError("Content file not found in source or output: {RelativeFilePath}", relativeFilePath);
        return null;
    }

    private async Task<string?> LoadContentFromPathAsync(string fullPath)
    {
        try
        {
            var content = await File.ReadAllTextAsync(fullPath);
            _logger.LogDebug("Loaded content from {FilePath}", fullPath);
            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading content from {FilePath}", fullPath);
            return null;
        }
    }
}

