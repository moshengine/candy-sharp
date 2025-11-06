using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Candy.Unity.Editor
{
    public static class ToolbarCallback
    {
        static Type m_toolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
        static Type m_guiViewType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GUIView");
        
        #if UNITY_2020_1_OR_NEWER
        static Type m_iWindowBackendType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.IWindowBackend");
        static PropertyInfo m_windowBackend = m_guiViewType.GetProperty("windowBackend",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        static PropertyInfo m_viewVisualTree = m_iWindowBackendType.GetProperty("visualTree",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        #else
        static PropertyInfo m_viewVisualTree = m_guiViewType.GetProperty("visualTree",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        #endif

        static FieldInfo m_imguiContainerOnGui = typeof(UnityEngine.UIElements.IMGUIContainer).GetField("m_OnGUIHandler",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        static ScriptableObject m_currentToolbar;

        /// <summary>
        /// Callback for toolbar OnGUI method.
        /// </summary>
        public static Action OnToolbarGUI;
        public static Action OnToolbarGUILeft;
        public static Action OnToolbarGUIRight;

        static ToolbarCallback()
        {
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
        }

        static void OnUpdate()
        {
            // Relying on the fact that toolbar is ScriptableObject and gets deleted when layout changes
            if (m_currentToolbar == null)
            {
                // Find toolbar
                var toolbars = Resources.FindObjectsOfTypeAll(m_toolbarType);
                m_currentToolbar = toolbars.Length > 0 ? (ScriptableObject)toolbars[0] : null;
                if (m_currentToolbar != null)
                {
                    #if UNITY_2021_1_OR_NEWER
                    var root = m_currentToolbar.GetType().GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
                    var rawRoot = root.GetValue(m_currentToolbar);
                    var mRoot = rawRoot as UnityEngine.UIElements.VisualElement;
                    RegisterCallback("ToolbarZoneLeftAlign", OnToolbarGUILeft);
                    RegisterCallback("ToolbarZoneRightAlign", OnToolbarGUIRight);

                    void RegisterCallback(string root, Action cb)
                    {
                        var toolbarZone = mRoot.Q(root);

                        if (toolbarZone != null)
                        {
                            var parent = new UnityEngine.UIElements.VisualElement()
                            {
                                style = {
                                    flexGrow = 1,
                                    flexDirection = UnityEngine.UIElements.FlexDirection.Row,
                                }
                            };
                            var container = new UnityEngine.UIElements.IMGUIContainer();
                            container.style.flexGrow = 1;
                            container.onGUIHandler += () => { cb?.Invoke(); };
                            parent.Add(container);
                            toolbarZone.Add(parent);
                        }
                    }
                    #else
                    
                    #if UNITY_2020_1_OR_NEWER
                    var windowBackend = m_windowBackend.GetValue(m_currentToolbar);

                    // Get visual tree from the window backend
                    var visualTree = (UnityEngine.UIElements.VisualElement)m_viewVisualTree.GetValue(windowBackend, null);
                    #else
                    // Get visual tree directly
                    var visualTree = (UnityEngine.UIElements.VisualElement)m_viewVisualTree.GetValue(m_currentToolbar, null);
                    #endif

                    // Get first child which should be the toolbar IMGUIContainer
                    var container = (UnityEngine.UIElements.IMGUIContainer)visualTree[0];

                    // (Re)attach handler
                    var handler = (Action)m_imguiContainerOnGui.GetValue(container);
                    handler -= OnGUI;
                    handler += OnGUI;
                    m_imguiContainerOnGui.SetValue(container, handler);

                    #endif
                }
            }
        }

        static void OnGUI()
        {
            var screenWidth = EditorGUIUtility.currentViewWidth;

            // Following calculations match code reflected from Toolbar.OldOnGUI()
            float playButtonsPosition = Mathf.RoundToInt((screenWidth - 100) / 2);

            Rect leftRect = new Rect(0, 0, screenWidth, Screen.height);
            leftRect.xMin += 10; // Spacing left
            leftRect.xMin += 32 * 2; // File + Edit
            leftRect.xMax = playButtonsPosition;

            Rect rightRect = new Rect(0, 0, screenWidth, Screen.height);
            rightRect.xMin = playButtonsPosition;
            rightRect.xMin += 32 * 3; // Play buttons
            rightRect.xMin += 140; // Platform + player dropdowns
            rightRect.xMax = screenWidth;
            rightRect.xMax -= 10; // Spacing right
            rightRect.xMax -= 80; // Cloud
            rightRect.xMax -= 10; // Spacing between cloud and account
            rightRect.xMax -= 32; // Account
            rightRect.xMax -= 10; // Spacing between account and ...
            rightRect.xMax -= 24; // ...
            rightRect.xMax -= 10; // Spacing between ... and collab

            // Add spacing around existing controls
            leftRect.xMin += 10;
            leftRect.xMax -= 10;
            rightRect.xMin += 10;
            rightRect.xMax -= 10;

            // Left toolbar
            if (leftRect.width > 0)
            {
                GUILayout.BeginArea(leftRect);
                GUILayout.BeginHorizontal();
                OnToolbarGUILeft?.Invoke();
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }

            // Right toolbar
            if (rightRect.width > 0)
            {
                GUILayout.BeginArea(rightRect);
                GUILayout.BeginHorizontal();
                OnToolbarGUIRight?.Invoke();
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }

            OnToolbarGUI?.Invoke();
        }
    }
}

