using System;
using System.Collections.Generic;

namespace Candy.Unity.Editor
{
#if !UNITY_6000_3_OR_NEWER
    /// <summary>
    /// Legacy API for custom toolbar implementations (Unity < 6.3).
    /// For Unity 6.3+, use the official MainToolbarElement API instead.
    /// See: https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Toolbars.MainToolbar.html
    /// </summary>
    public static class CustomToolbar
    {
        /// <summary>
        /// List of GUI handlers to be rendered on the left side of the toolbar.
        /// Add your custom GUI rendering callbacks to this list.
        /// </summary>
        public static List<Action> LeftToolbarGUI => ToolbarExtender.LeftToolbarGUI;

        /// <summary>
        /// List of GUI handlers to be rendered on the right side of the toolbar.
        /// Add your custom GUI rendering callbacks to this list.
        /// </summary>
        public static List<Action> RightToolbarGUI => ToolbarExtender.RightToolbarGUI;
    }
#endif
}
