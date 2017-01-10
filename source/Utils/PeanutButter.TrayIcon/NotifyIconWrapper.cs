using System;
using System.Drawing;
using System.Windows.Forms;

namespace PeanutButter.TrayIcon
{
    internal interface INotifyIcon: IDisposable
    {
        NotifyIcon Actual { get; }
        Icon Icon { get; set; }
        bool Visible { get; set; }
        ContextMenu ContextMenu { get; set; }
        void ShowBalloonTip(int timeout, string tipTitle, string tipText, ToolTipIcon tipIcon);
    }

    internal class NotifyIconWrapper: INotifyIcon
    {
        public NotifyIcon Actual { get; }
        public Icon Icon
        {
            get { return Actual.Icon; }
            set { Actual.Icon = value; }
        }
        public bool Visible
        {
            get { return Actual.Visible; }
            set { Actual.Visible = value; }
        }

        public ContextMenu ContextMenu
        {
            get { return Actual.ContextMenu; }
            set { Actual.ContextMenu = value; }
        }

        public NotifyIconWrapper(NotifyIcon actual)
        {
            Actual = actual;
        }

        public void ShowBalloonTip(int timeout, string tipTitle, string tipText, ToolTipIcon tipIcon)
        {
            Actual.ShowBalloonTip(timeout, tipTitle, tipText, tipIcon);
        }

        public void Dispose()
        {
            Actual?.Dispose();
        }
    }
}