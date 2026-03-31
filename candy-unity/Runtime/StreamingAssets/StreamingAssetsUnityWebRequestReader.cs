using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Candy.Unity
{
    /// <summary>
    /// StreamingAssets exposed as a URL (not a real file path): Android APK (<c>jar:file:...!/assets/...</c>) and WebGL.
    /// Unity requires <see cref="UnityWebRequest"/> to read these; <see cref="System.IO.File"/> does not work.
    /// </summary>
    public sealed class StreamingAssetsUnityWebRequestReader : IStreamingAssetsTextReader
    {
        public string ReadAllText(string absolutePath)
        {
            string url = NormalizeUrlForWebRequest(absolutePath);
            using var request = UnityWebRequest.Get(url);
            var op = request.SendWebRequest();
            while (!op.isDone) { }
            if (request.result != UnityWebRequest.Result.Success)
                throw new IOException($"UnityWebRequest failed: {request.error}");
            return request.downloadHandler.text;
        }

        public async Task<string> ReadAllTextAsync(string absolutePath)
        {
            string url = NormalizeUrlForWebRequest(absolutePath);
            using var request = UnityWebRequest.Get(url);
            var op = request.SendWebRequest();
            while (!op.isDone)
                await Task.Yield();
            if (request.result != UnityWebRequest.Result.Success)
                throw new IOException($"UnityWebRequest failed: {request.error}");
            return request.downloadHandler.text;
        }

        private static string NormalizeUrlForWebRequest(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;
            path = path.Replace('\\', '/');
            int marker = path.IndexOf("!/assets/", StringComparison.Ordinal);
            if (marker >= 0)
            {
                string prefix = path.Substring(0, marker + 9);
                string rest = path.Substring(marker + 9);
                var segments = rest.Split('/');
                for (int i = 0; i < segments.Length; i++)
                {
                    if (segments[i].Length > 0)
                        segments[i] = Uri.EscapeDataString(segments[i]);
                }
                return prefix + string.Join("/", segments);
            }
            return path.Replace(" ", "%20");
        }
    }
}
