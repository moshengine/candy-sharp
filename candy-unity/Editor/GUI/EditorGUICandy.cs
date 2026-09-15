using UnityEditor;
using UnityEngine;

namespace MoshEngine.Candy.Unity.Editor
{
    public static class EditorGUICandy
    {
        public static void BeginLevelEditor()
        {
            UpdateGUIEnabledBasedOnPlayMode();
        }

        public static void EndLevelEditor()
        {
            EnableGUI();
        }

        public static void BeginSection(string title, bool topPadding = true)
        {
            if (topPadding)
            {
                EditorGUILayout.Space(5);
            }

            var wrapperBoxStyle = new GUIStyle(EditorStyles.helpBox) { padding = new RectOffset(-10, -10, -10, -10) };
            EditorGUILayout.BeginVertical(wrapperBoxStyle);

            var titleBoxStyle = new GUIStyle("box") { padding = new RectOffset(7, 5, 5, 5) };
            var labelStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 14, normal = { textColor = Color.white } };
            EditorGUILayout.BeginVertical(titleBoxStyle);
            EditorGUILayout.LabelField(title, labelStyle, GUILayout.Height(20));
            EditorGUILayout.EndVertical();

            var contentBoxStyle = new GUIStyle { padding = new RectOffset(10, 10, 10, 10) };
            EditorGUILayout.BeginVertical(contentBoxStyle);
        }

        public static void EndSection()
        {
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        public static bool Button(string text, string icon = null, int height = 30, bool enabled = true)
        {
            if (!enabled) DisableGUI();
            GUIContent content = icon != null
                ? new GUIContent(" " + text, EditorGUIUtility.IconContent(icon).image)
                : new GUIContent(text);
            bool result = GUILayout.Button(content, GUILayout.Height(height));
            UpdateGUIEnabledBasedOnPlayMode();
            return result;
        }

        public static void Property(SerializedObject serializedObject, string fieldName)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(fieldName));
        }

        public static void Property(SerializedProperty serializedProperty, string fieldName)
        {
            EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative(fieldName));
        }

        public static void Property(SerializedProperty serializedProperty, string fieldName, string labelOverride = null)
        {
            if (labelOverride != null)
            {
                EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative(fieldName), new GUIContent(labelOverride));
            }
            else
            {
                EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative(fieldName));
            }
        }

        public static void BeginHorizontal()
        {
            EditorGUILayout.BeginHorizontal();
        }

        public static void EndHorizontal()
        {
            EditorGUILayout.EndHorizontal();
        }

        private static void EnableGUI()
        {
            GUI.enabled = true;
        }

        private static void UpdateGUIEnabledBasedOnPlayMode()
        {
            GUI.enabled = !Application.isPlaying;
        }

        private static void DisableGUI()
        {
            GUI.enabled = false;
        }
    }
}