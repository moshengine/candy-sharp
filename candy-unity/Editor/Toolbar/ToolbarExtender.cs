using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Candy.Unity.Editor
{
#if !UNITY_6000_3_OR_NEWER
    /// <summary>
    /// Legacy toolbar extender for Unity versions before 6.3.
    /// For Unity 6.3+, use the official MainToolbarElement API instead.
    /// </summary>
    [InitializeOnLoad]
    public static class ToolbarExtender
    {
        static int toolCount;

        public static readonly List<Action> LeftToolbarGUI = new List<Action>();
        public static readonly List<Action> RightToolbarGUI = new List<Action>();

        static ToolbarExtender()
        {
            Type toolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
            
            #if UNITY_2021_1_OR_NEWER
            string fieldName = "k_ToolCount";
            #else
            string fieldName = "get_lastLoadedLayoutName";
            #endif

            FieldInfo toolCountField = toolbarType.GetField(fieldName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            
            if (toolCountField != null)
            {
                toolCount = toolCountField != null ? ((int)toolCountField.GetValue(null)) : 8;
            }
            else
            {
                toolCount = 8;
            }

            ToolbarCallback.OnToolbarGUI = OnGUI;
            ToolbarCallback.OnToolbarGUILeft = GUILeft;
            ToolbarCallback.OnToolbarGUIRight = GUIRight;
        }

        private static void OnGUI()
        {
            // This is called when the toolbar is drawn
        }

        private static void GUILeft()
        {
            GUILayout.BeginHorizontal();
            foreach (var handler in LeftToolbarGUI)
            {
                handler();
            }
            GUILayout.EndHorizontal();
        }

        private static void GUIRight()
        {
            GUILayout.BeginHorizontal();
            foreach (var handler in RightToolbarGUI)
            {
                handler();
            }
            GUILayout.EndHorizontal();
        }
    }
#endif
}
