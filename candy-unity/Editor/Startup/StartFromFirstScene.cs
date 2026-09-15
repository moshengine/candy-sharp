using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace MoshEngine.Candy.Unity
{
  public class StartFromFirstScene
  {
    [MenuItem("Tools/Candy/Start From First Scene %#T")]
    public static void StartPlayModeFromFirstScene()
    {
      EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
      EditorSceneManager.OpenScene(SceneUtility.GetScenePathByBuildIndex(0));
      EditorApplication.EnterPlaymode();
    }
  }
}
