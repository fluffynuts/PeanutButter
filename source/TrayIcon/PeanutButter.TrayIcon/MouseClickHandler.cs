using System;
using System.Windows.Forms;

namespace PeanutButter.TrayIcon
{
    /// <summary>
    /// Enumeration for number of mouse clicks
    /// </summary>
    public enum MouseClicks
    {
        /// <summary>
        /// Single-Click
        /// </summary>
        Single,
        /// <summary>
        /// Double-Click
        /// </summary>
        Double
    }

    /// <summary>
    /// Object wrapping a mouse click handler
    /// </summary>
    public class MouseClickHandler
    {
        /// <summary>
        /// Which button this handler is for
        /// </summary>
        public MouseButtons Button { get; }
        /// <summary>
        /// The action to run when the handler is matched
        /// </summary>
        public Action Action { get; }
        /// <summary>
        /// Run on single or double-click
        /// </summary>
        public MouseClicks Clicks { get; }

        /// <summary>
        /// Constructs a MouseClickHandler registration container
        /// </summary>
        /// <param name="clicks">Single- or Double- click handling</param>
        /// <param name="button">Mouse button to handle for</param>
        /// <param name="action">Action to run when the handler is matched</param>
        public MouseClickHandler(MouseClicks clicks, MouseButtons button, Action action)
        {
            Clicks = clicks;
            Button = button;
            Action = action;
        }
    }
}