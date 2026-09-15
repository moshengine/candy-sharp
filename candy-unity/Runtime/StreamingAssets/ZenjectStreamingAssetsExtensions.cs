using Zenject;

namespace MoshEngine.Candy.Unity
{
    public static class ZenjectStreamingAssetsExtensions
    {
        public static void BindStreamingAssetsTextReader(this DiContainer container)
        {
            container.Bind<IStreamingAssetsTextReader>().FromMethod(_ => StreamingAssetsTextReaderFactory.Create()).AsSingle();
        }
    }
}
