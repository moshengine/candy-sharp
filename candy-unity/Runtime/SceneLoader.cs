using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Candy.Unity
{
  /// <summary>
  ///   Responsible for managing the transition between scenes.
  /// </summary>
  public class SceneLoader : MonoBehaviour
  {
    public string CurrentScene { get; private set; }

    /// <summary>
    ///   Switches to a new scene, while displaying a loading screen.
    /// </summary>
    /// <param name="newScene">The name of the new scene to switch to.</param>
    /// <param name="loadingScreenScene">The name of the loading screen scene.</param>
    public void SwitchScene(string newScene, string loadingScreenScene)
    {
      StartCoroutine(SwitchScene_Co(newScene, loadingScreenScene));
    }

    private IEnumerator SwitchScene_Co(string newScene, string loadingScreenScene)
    {
      // Load the loading screen scene additively
      // var loadingScreenSceneLoad = SceneManager.LoadSceneAsync(loadingScreenScene, LoadSceneMode.Additive);
      // while (!loadingScreenSceneLoad.isDone)
      // {
      //   yield return null;
      // }

      // Load the new scene additively
      var newSceneLoad = SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Additive);
      while (!newSceneLoad.isDone)
      {
        yield return null;
      }

      // Unload the current scene if it exists
      if (!string.IsNullOrEmpty(CurrentScene))
      {
        var oldSceneUnload = SceneManager.UnloadSceneAsync(CurrentScene);
        while (!oldSceneUnload.isDone)
        {
          yield return null;
        }
      }

      // Unload the loading screen scene
      // SceneManager.UnloadSceneAsync(loadingScreenScene);

      // Set the new scene as the active scene
      CurrentScene = newScene;
      SceneManager.SetActiveScene(SceneManager.GetSceneByName(CurrentScene));
    }
  }
}
