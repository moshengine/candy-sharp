using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PathToAscension.Development
{
    public class ResolutionSwitcher : MonoBehaviour
    {
        [SerializeField]
        private ResolutionConfig[] resolutionConfigurations = new ResolutionConfig[0];

        private void Update()
        {
            foreach (var resolutionConfig in this.resolutionConfigurations)
            {
                if (Keyboard.current[resolutionConfig.Input].wasPressedThisFrame)
                {
                    var fullscreenMode = resolutionConfig.Fullscreen
                        ? FullScreenMode.FullScreenWindow
                        : FullScreenMode.Windowed;
                    Screen.SetResolution(resolutionConfig.Width, resolutionConfig.Height, fullscreenMode);
                }
            }
        }

        [Serializable]
        class ResolutionConfig
        {
            public Key Input = Key.None;
            public int Width = 0;
            public int Height = 0;
            public bool Fullscreen = false;
        }
    }
}
