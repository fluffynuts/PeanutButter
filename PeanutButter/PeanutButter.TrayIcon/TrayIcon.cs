using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace PeanutButter.TrayIcon
{
    public class TrayIcon: IDisposable
    {
        public NotifyIcon NotifyIcon { get { return _notificationIcon; } }
		private NotifyIcon _notificationIcon;
        private Icon _icon;
        private bool _showingDefaultBalloonTip;
        public int DefaultBalloonTipTimeout { get; set; }

        public Icon Icon
        {
            get
            {
                return _icon;
            }
            set
            {
                _icon = value;
                _notificationIcon.Icon = _icon;
            }
        }

        public string DefaultTipText { get; set; }
        public string DefaultTipTitle { get; set; }
        public Action DefaultBalloonTipClickedAction { get; set; }
        public Action DefaultBalloonTipClosedAction { get; set; }

        public TrayIcon(Icon icon)
        {
            Init(icon);
        }

        public TrayIcon(Stream iconImageStream)
        {
            InitWithStream(iconImageStream);
        }

        private void InitWithStream(Stream iconImageStream)
        {
            var icon = new Icon(iconImageStream);
            Init(icon);
        }

        public TrayIcon(Bitmap iconBitmap)
        {
            Init(Icon.FromHandle(iconBitmap.GetHicon()));
        }

        public TrayIcon(string pathToIcon)
        {
            using (var fileStream = new FileStream(pathToIcon, FileMode.Open, FileAccess.Read))
            {
                InitWithStream(fileStream);
            }
        }

        private void Init(Icon icon)
        {
            this._icon = icon;
            DefaultBalloonTipTimeout = 2000;
            _notificationIcon = new NotifyIcon();
            _notificationIcon.ContextMenu = new ContextMenu();
            _notificationIcon.MouseMove += ShowDefaultBalloonTip;
            _notificationIcon.BalloonTipClicked += BalloonTipClickedHandler;
            _notificationIcon.BalloonTipClosed += BalloonTipClosedHandler;
        }

        private void BalloonTipClosedHandler(object sender, EventArgs e)
        {
            Action toRun = GetCustomBalloonTipClosedAction();
            if (toRun == null)
            {
                toRun = DefaultBalloonTipClosedAction;
            }
            if (toRun != null)
                toRun();
        }

        private void BalloonTipClickedHandler(object sender, EventArgs e)
        {
            Action toRun = GetCustomBalloonTipClickAction();
            if (toRun == null)
            {
                toRun = DefaultBalloonTipClickedAction;
            }
            if (toRun != null)
                toRun();
        }

        private void ShowDefaultBalloonTip(object sender, MouseEventArgs e)
        {
            if (String.IsNullOrEmpty(DefaultTipText) || String.IsNullOrEmpty(DefaultTipTitle)) return;
            lock (this)
            {
                if (HaveRegisteredClickHandlers()) return;
                if (_showingDefaultBalloonTip) return;
                _showingDefaultBalloonTip = true;
            }
            ShowBalloonTipFor(DefaultBalloonTipTimeout, DefaultTipTitle, DefaultTipText, ToolTipIcon.Info, DefaultBalloonTipClickedAction,
                () =>
                {
                    _showingDefaultBalloonTip = false;
                    _balloonTipClickHandlers = null;
                    var closedAction = DefaultBalloonTipClosedAction;
                    if (closedAction != null)
                        closedAction();
                });
        }

        private bool HaveRegisteredClickHandlers()
        {
            lock (this)
            {
                return CustomBalloonTipHandlerExists_UNLOCKED();
            }
        }

        private bool CustomBalloonTipHandlerExists_UNLOCKED()
        {
            return (_balloonTipClickHandlers != null);
        }

        private Action GetCustomBalloonTipClickAction()
        {
            lock (this)
            {
                if (!CustomBalloonTipHandlerExists_UNLOCKED()) return null;
                return _balloonTipClickHandlers.ClickAction;
            }
        }

        private Action GetCustomBalloonTipClosedAction()
        {
            lock (this)
            {
                if (!CustomBalloonTipHandlerExists_UNLOCKED()) return null;
                return _balloonTipClickHandlers.ClosedAction;
            }
        }

        private class BalloonTipClickHandlerRegistration
        {
            public Action ClickAction { get; protected set; }
            public Action ClosedAction { get; protected set; }

            public BalloonTipClickHandlerRegistration(Action clickAction = null, Action closedAction = null)
            {
                ClickAction = clickAction;
                ClosedAction = closedAction;
            }
        }

        private BalloonTipClickHandlerRegistration _balloonTipClickHandlers;

        public void ShowBalloonTipFor(int timeoutInMilliseconds, string title, string text, ToolTipIcon icon,
            Action clickAction = null, Action closeAction = null)
        {
            lock (this)
            {
                _balloonTipClickHandlers = new BalloonTipClickHandlerRegistration(clickAction, closeAction);
            }
            _notificationIcon.ShowBalloonTip(timeoutInMilliseconds, title, text, icon);
        }


        public void AddMenuItem(string withText, Action withCallback)
        {
            lock(this)
            {
                if (_notificationIcon == null) return;
                var menuItem = new MenuItem()
                               {
                                   Text = withText
                               };
                if (withCallback != null)
                    menuItem.Click += (s, e) => withCallback();
                _notificationIcon.ContextMenu.MenuItems.Add(menuItem);
            }
        }

        public void AddMenuSeparator()
        {
            AddMenuItem("-", null);
        }

        public void RemoveMenuItem(string withText)
        {
            lock(this)
            {
                if (_notificationIcon == null) return;
                foreach (MenuItem mi in _notificationIcon.ContextMenu.MenuItems)
                {
                    if (mi.Text == withText)
                    {
                        _notificationIcon.ContextMenu.MenuItems.Remove(mi);
                        return;
                    }
                }
            }
        }

        public void Show()
		{
			_notificationIcon.MouseClick += onIconMouseClick;
			_notificationIcon.Icon = _icon;
			_notificationIcon.Visible = true;
		}

        public void Hide()
        {
            _notificationIcon.Visible = false;
        }

        public void Dispose()
		{
            lock(this)
            {
                if (_notificationIcon != null)
    			    _notificationIcon.Dispose();
                _notificationIcon = null;
            }
		}

		void onIconMouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
			}
		}
	}
}
