using System.IO;
using System.Threading.Tasks;

namespace Candy.Unity
{
    /// <summary>
    /// StreamingAssets backed by a normal filesystem path (Editor, Windows/macOS/Linux standalone, iOS, etc.).
    /// </summary>
    public sealed class StreamingAssetsFileReader : IStreamingAssetsTextReader
    {
        public string ReadAllText(string absolutePath)
        {
            if (!File.Exists(absolutePath))
                throw new FileNotFoundException("StreamingAssets file not found.", absolutePath);
            return File.ReadAllText(absolutePath);
        }

        public Task<string> ReadAllTextAsync(string absolutePath)
        {
            if (!File.Exists(absolutePath))
                return Task.FromException<string>(new FileNotFoundException("StreamingAssets file not found.", absolutePath));
            return Task.FromResult(File.ReadAllText(absolutePath));
        }
    }
}
