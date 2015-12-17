using System;

namespace PeanutButter.TrayIcon
{
    internal class BalloonTipClickHandlerRegistration
    {
        public Action ClickAction { get; protected set; }
        public Action ClosedAction { get; protected set; }

        public BalloonTipClickHandlerRegistration(Action clickAction = null, Action closedAction = null)
        {
            ClickAction = clickAction;
            ClosedAction = closedAction;
        }
    }
}