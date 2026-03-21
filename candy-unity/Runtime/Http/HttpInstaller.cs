using UnityEngine;
using Zenject;

namespace Candy.Unity
{
    public class HttpInstaller : MonoInstaller
    {
        [SerializeField]
        private HttpClient _httpClient;

        public override void InstallBindings()
        {
            Container.Bind<HttpClient>().FromInstance(_httpClient).AsSingle();
        }
    }
}
