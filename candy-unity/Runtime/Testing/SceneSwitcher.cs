using General.Utility;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace PathToAscension.Development
{
    public class SceneSwitcher : MonoBehaviour
    {
        [SerializeField]
        private Key previousSceneKey = Key.None;

        [SerializeField]
        private Key nextSceneKey = Key.None;

        [SerializeField]
        private Key reloadSceneKey = Key.None;

        private void Update()
        {
            if (Keyboard.current[this.previousSceneKey].wasPressedThisFrame)
            {
                SceneManager.LoadScene(
                    (SceneManager.GetActiveScene().buildIndex - 1).Mod(SceneManager.sceneCountInBuildSettings)
                );
            }
            else if (Keyboard.current[this.nextSceneKey].wasPressedThisFrame)
            {
                SceneManager.LoadScene(
                    (SceneManager.GetActiveScene().buildIndex + 1).Mod(SceneManager.sceneCountInBuildSettings)
                );
            }
            else if (Keyboard.current[this.reloadSceneKey].wasPressedThisFrame)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}
