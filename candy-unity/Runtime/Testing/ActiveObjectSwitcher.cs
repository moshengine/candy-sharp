using UnityEngine;

namespace MoshEngine.Candy.Unity
{
    /**
     * Useful for testing purposes.
     */
    public class ActiveObjectSwitcher : MonoBehaviour
    {
        [SerializeField]
        private KeyCode key = KeyCode.None;

        [SerializeField]
        private GameObject[] gameObjects = new GameObject[0];

        private int activeIndex = 0;

        private void Start()
        {
            for (int i = 0; i < this.gameObjects.Length; i++)
            {
                if (i == 0)
                {
                    this.gameObjects[i].SetActive(true);
                }
                else
                {
                    this.gameObjects[i].SetActive(false);
                }
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(this.key))
            {
                this.gameObjects[this.activeIndex].SetActive(false);
                this.activeIndex = (this.activeIndex + 1) % this.gameObjects.Length;
                this.gameObjects[this.activeIndex].SetActive(true);
            }
        }
    }
}
