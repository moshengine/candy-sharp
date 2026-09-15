using UnityEditor;
using UnityEngine;

namespace MoshEngine.Candy.Unity.Editor
{
    [CustomPropertyDrawer(typeof(HelpBoxAttribute))]
    public class HelpBoxDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var helpBox = attribute as HelpBoxAttribute;
            var messageType = helpBox.Type switch
            {
                HelpBoxType.Warning => MessageType.Warning,
                HelpBoxType.Error => MessageType.Error,
                _ => MessageType.Info
            };

            var helpBoxHeight = GetHelpBoxHeight(helpBox.Text);
            var helpBoxRect = new Rect(position.x, position.y, position.width, helpBoxHeight);
            var propertyRect = new Rect(position.x, position.y + helpBoxHeight + 2, position.width, EditorGUI.GetPropertyHeight(property));

            EditorGUI.HelpBox(helpBoxRect, helpBox.Text, messageType);
            EditorGUI.PropertyField(propertyRect, property, label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var helpBox = attribute as HelpBoxAttribute;
            var helpBoxHeight = GetHelpBoxHeight(helpBox.Text);
            return helpBoxHeight + EditorGUI.GetPropertyHeight(property) + 2;
        }

        private float GetHelpBoxHeight(string text)
        {
            var style = GUI.skin.GetStyle("helpbox");
            var content = new GUIContent(text);
            var height = style.CalcHeight(content, EditorGUIUtility.currentViewWidth - 20);
            return Mathf.Max(40, height);
        }
    }
}

