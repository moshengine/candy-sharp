using System;
using UnityEngine;

namespace MoshEngine.Candy.Unity
{
    public enum HelpBoxType
    {
        Info,
        Warning,
        Error
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class HelpBoxAttribute : PropertyAttribute
    {
        public string Text { get; }
        public HelpBoxType Type { get; }

        public HelpBoxAttribute(string text, HelpBoxType type = HelpBoxType.Info)
        {
            Text = text;
            Type = type;
        }
    }
}

