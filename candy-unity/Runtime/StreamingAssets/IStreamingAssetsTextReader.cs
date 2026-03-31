using System.Threading.Tasks;

namespace Candy.Unity
{
    /// <summary>
    /// Reads text from paths under <see cref="UnityEngine.Application.streamingAssetsPath"/>.
    /// Implementations differ: filesystem (Editor, desktop, iOS) vs URL-based (Android APK, WebGL).
    /// </summary>
    public interface IStreamingAssetsTextReader
    {
        string ReadAllText(string absolutePath);

        Task<string> ReadAllTextAsync(string absolutePath);
    }
}
