using System;
using System.Collections.Generic;

namespace Candy.Unity.Editor
{
    /// <summary>
    /// Provides a convenient API for custom toolbar implementations.
    /// Game-specific code can directly add GUI handlers to these lists,
    /// which are passed through to the underlying ToolbarExtender.
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
}
