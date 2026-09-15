using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Tilemaps;
using UnityEngine;

namespace MoshEngine.Candy.Unity.Editor
{
    public abstract class GameBrushEditorBase : GridBrushEditorBase
    {
        protected abstract GameObject Prefab { get; set; }
        protected abstract string PrefabFieldName { get; }
        protected abstract System.Type RequiredComponentType { get; }

        private Dictionary<int, Texture2D> _previewCache = new Dictionary<int, Texture2D>();

        public override void OnPaintInspectorGUI()
        {
            Prefab = (GameObject)EditorGUILayout.ObjectField(PrefabFieldName, Prefab, typeof(GameObject), false);

            if (Prefab == null || Prefab.GetComponent(RequiredComponentType) == null)
            {
                EditorGUILayout.HelpBox($"Please assign a {PrefabFieldName} to use this brush.", MessageType.Warning);
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Ignore", EditorStyles.boldLabel);
        }

        public override bool canChangeZPosition => false;

        protected Texture2D GetCachedPrefabPreview(GameObject prefab, int size)
        {
            int prefabId = prefab.GetInstanceID();
            if (_previewCache.TryGetValue(prefabId, out Texture2D cachedPreview) && cachedPreview != null)
            {
                return cachedPreview;
            }

            Texture2D newPreview = GetPrefabPreview(prefab, size);
            _previewCache[prefabId] = newPreview;
            return newPreview;
        }

        [InitializeOnLoadMethod]
        private static void RegisterPlayModeStateChanged()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                ClearPreviewCache();
            }
        }

        private static void ClearPreviewCache()
        {
            foreach (var editor in Resources.FindObjectsOfTypeAll<GameBrushEditorBase>())
            {
                editor._previewCache.Clear();
            }
        }

        private Texture2D GetPrefabPreview(GameObject prefab, int size)
        {
            // Create a temporary scene to instantiate the prefab
            var previewScene = EditorSceneManager.NewPreviewScene();
            var instance = PrefabUtility.InstantiatePrefab(prefab, previewScene) as GameObject;

            if (instance != null)
            {
                // Ensure all renderers are visible
                var renderers = instance.GetComponentsInChildren<Renderer>(true);
                foreach (var renderer in renderers)
                {
                    renderer.gameObject.SetActive(true);
                }

                // Generate preview
                var preview = AssetPreview.GetAssetPreview(instance);

                // Clean up
                Object.DestroyImmediate(instance);
                EditorSceneManager.ClosePreviewScene(previewScene);

                // If preview generation was successful, return it
                if (preview != null)
                {
                    return preview;
                }
            }

            // If preview generation failed, return a default texture
            return CreateDefaultPreviewTexture(size);
        }

        private Texture2D CreateDefaultPreviewTexture(int size)
        {
            Texture2D defaultTexture = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.gray;
            }
            defaultTexture.SetPixels(pixels);
            defaultTexture.Apply();
            return defaultTexture;
        }

        protected virtual void OnDisable()
        {
            // Clear the cache when the editor is disabled
            _previewCache.Clear();
        }

        protected void RenderPrefabPreviews<T>(IEnumerable<T> prefabs, System.Func<T, GameObject> getPrefabFunc)
        {
            EditorGUILayout.BeginHorizontal();
            foreach (var item in prefabs)
            {
                GameObject prefab = getPrefabFunc(item);
                EditorGUILayout.BeginVertical(GUILayout.Width(80));

                // Display preview of the prefab
                Texture2D preview = GetCachedPrefabPreview(prefab, 80);
                if (GUILayout.Button(preview, GUILayout.Width(80), GUILayout.Height(80)))
                {
                    Prefab = prefab;
                }

                // Prefab name under the button
                EditorGUILayout.LabelField(prefab.name, EditorStyles.centeredGreyMiniLabel, GUILayout.Width(80));

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
