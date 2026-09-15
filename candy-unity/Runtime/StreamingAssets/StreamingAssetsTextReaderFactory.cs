using UnityEngine;

namespace MoshEngine.Candy.Unity
{
    /// <summary>
    /// Chooses the correct <see cref="IStreamingAssetsTextReader"/> for the current platform.
    /// </summary>
    public static class StreamingAssetsTextReaderFactory
    {
        public static IStreamingAssetsTextReader Create()
        {
#if UNITY_EDITOR
            return new StreamingAssetsFileReader();
#else
            return Application.platform switch
            {
                RuntimePlatform.Android => new StreamingAssetsUnityWebRequestReader(),
                RuntimePlatform.WebGLPlayer => new StreamingAssetsUnityWebRequestReader(),
                _ => new StreamingAssetsFileReader(),
            };
#endif
        }
    }
}
