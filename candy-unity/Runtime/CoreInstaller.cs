using UnityEngine;
using Zenject;

namespace CandyCore
{
  public class CoreInstaller : MonoInstaller
  {
    [SerializeField] private SceneLoader _sceneLoader;

    public override void InstallBindings()
    {
      Container.Bind<SceneLoader>().FromInstance(_sceneLoader).AsSingle();
      Container.BindFactory<GameObject, Transform, int, ObjectPool<PoolableObject>, ObjectPoolFactory>();
    }
  }
}
