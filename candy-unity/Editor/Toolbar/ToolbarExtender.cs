using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Candy.Unity.Editor
{
    [InitializeOnLoad]
    public static class ToolbarExtender
    {
        static int toolCount;
        static GUIStyle commandStyle = null;

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
}

