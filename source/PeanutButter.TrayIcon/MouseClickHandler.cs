using System;
using System.Windows.Forms;

namespace PeanutButter.TrayIcon
{
    public enum MouseClicks
    {
        Single,
        Double
    }

    public class MouseClickHandler
    {
        public MouseButtons Button { get; private set; }
        public Action Action { get; private set; }
        public MouseClicks Clicks { get; private set; }

        public MouseClickHandler(MouseClicks clicks, MouseButtons button, Action action)
        {
            Clicks = clicks;
            Button = button;
            Action = action;
        }
    }
}