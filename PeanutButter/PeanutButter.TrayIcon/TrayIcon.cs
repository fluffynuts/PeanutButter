using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PeanutButter.TrayIcon
{
    public interface ITrayIcon
    {
        NotifyIcon NotifyIcon { get; }
        int DefaultBalloonTipTimeout { get; set; }
        Icon Icon { get; set; }
        string DefaultTipText { get; set; }
        string DefaultTipTitle { get; set; }
        Action DefaultBalloonTipClickedAction { get; set; }
        Action DefaultBalloonTipClosedAction { get; set; }

        void ShowBalloonTipFor(int timeoutInMilliseconds, string title, string text, ToolTipIcon icon,
            Action clickAction = null, Action closeAction = null);

        MenuItem AddSubMenu(string text, MenuItem parent = null);
        void AddMenuItem(string withText, Action withCallback, MenuItem parent = null);
        void AddMenuSeparator(MenuItem subMenu = null);
        void RemoveMenuItem(string withText);
        MouseClickHandler AddMouseClickHandler(MouseClicks clicks, MouseButtons button, Action handler);
        void RemoveMouseClickHandler(MouseClickHandler handler);

        void Init(Icon icon);
        void Init(Stream iconStream);
        void Init(string pathToIcon);
        void Init(Bitmap bitmap);
        void Show();
        void Hide();
        void Dispose();
    }

    public class TrayIcon: IDisposable, ITrayIcon
    {
        public NotifyIcon NotifyIcon { get { return _notificationIcon; } }
		private NotifyIcon _notificationIcon;
        private Icon _icon;
        private bool _showingDefaultBalloonTip;
        public int DefaultBalloonTipTimeout { get; set; }
        private object _lock = new object();
        private List<MouseClickHandler> _mouseClickHandlers = new List<MouseClickHandler>();

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

        public TrayIcon()
        {
        }

        public TrayIcon(Icon icon)
        {
            Init(icon);
        }

        public TrayIcon(Bitmap icon)
        {
            Init(icon);
        }

        public TrayIcon(Stream iconImageStream)
        {
            Init(iconImageStream);
        }

        public void Init(Stream iconImageStream)
        {
            var icon = new Icon(iconImageStream);
            Init(icon);
        }

        public TrayIcon(string pathToIcon)
        {
            Init(pathToIcon);
        }

        public void Init(string pathToIcon)
        {
            using (var fileStream = new FileStream(pathToIcon, FileMode.Open, FileAccess.Read))
            {
                Init(fileStream);
            }
        }

        public void Init(Bitmap bitmap)
        {
            Init(Icon.FromHandle(bitmap.GetHicon()));
        }

        private BalloonTipClickHandlerRegistration _balloonTipClickHandlers;
        private bool _alreadyInitialized;

        public void ShowBalloonTipFor(int timeoutInMilliseconds, string title, string text, ToolTipIcon icon,
            Action clickAction = null, Action closeAction = null)
        {
            lock (this)
            {
                _balloonTipClickHandlers = new BalloonTipClickHandlerRegistration(clickAction, closeAction);
            }
            _notificationIcon.ShowBalloonTip(timeoutInMilliseconds, title, text, icon);
        }

        public MenuItem AddSubMenu(string text, MenuItem parent = null)
        {
            lock (_lock)
            {
                if (_notificationIcon == null) return null;
                var menuItem = CreateMenuItemWithText(text);
                AddMenuToParentOrRoot(parent, menuItem);
                return menuItem;
            }
        }

        public void AddMenuItem(string withText, Action withCallback, MenuItem parent = null)
        {
            lock(_lock)
            {
                if (_notificationIcon == null) return;
                var menuItem = CreateMenuItemWithText(withText);
                if (withCallback != null)
                    menuItem.Click += (s, e) => withCallback();
                AddMenuToParentOrRoot(parent, menuItem);
            }
        }

        public void AddMenuSeparator(MenuItem subMenu = null)
        {
            AddSubMenu("-", subMenu);
        }

        public void RemoveMenuItem(string withText)
        {
            lock(_lock)
            {
                if (_notificationIcon == null) return;
                var toRemove = FindMenusByText(withText);
                foreach (var item in toRemove)
                {
                    item.Parent.MenuItems.Remove(item);
                }
            }
        }

        public void Show()
		{
			_notificationIcon.MouseClick += OnIconMouseClick;
            _notificationIcon.MouseDoubleClick += OnIconMouseDoubleClick;
			_notificationIcon.Icon = _icon;
			_notificationIcon.Visible = true;
		}

        public void Hide()
        {
            _notificationIcon.Visible = false;
        }

        public void Dispose()
		{
            lock(_lock)
            {
                if (_notificationIcon != null)
    			    _notificationIcon.Dispose();
                _notificationIcon = null;
            }
		}

        public MouseClickHandler AddMouseClickHandler(MouseClicks clicks, MouseButtons button, Action handler)
        {
            lock (_lock)
            {
                var handlerItem = new MouseClickHandler(clicks, button, handler);
                _mouseClickHandlers.Add(handlerItem);
                return handlerItem;
            }
        }

        public void RemoveMouseClickHandler(MouseClickHandler handler)
        {
            lock (_lock)
            {
                _mouseClickHandlers.Remove(handler);
            }
        }

        private void AddMenuToParentOrRoot(MenuItem parent, MenuItem menuItem)
        {
            var addTo = parent as Menu ?? _notificationIcon.ContextMenu;
            addTo.MenuItems.Add(menuItem);
        }

        private void OnIconMouseClick(object sender, MouseEventArgs e)
		{
		    var handlers = FindHandlersFor(MouseClicks.Single, e.Button);
		    RunMouseClickHandlers(handlers);
		}

        private void OnIconMouseDoubleClick(object sender, MouseEventArgs e)
        {
            var handlers = FindHandlersFor(MouseClicks.Double, e.Button);
            RunMouseClickHandlers(handlers);
        }

        private void RunMouseClickHandlers(MouseClickHandler[] handlers)
        {
            var exceptions = handlers
                .Select(handler => TryDo(handler.Action))
                .Where(ex => ex != null);
            if (exceptions.Any())
                throw new AggregateException(exceptions);
        }

        private Exception TryDo(Action action)
        {
            try
            {
                action();
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        private MouseClickHandler[] FindHandlersFor(MouseClicks clicks, MouseButtons button)
        {
		    lock (_lock)
		    {
		        return _mouseClickHandlers
                        .Where(o => o.Clicks == MouseClicks.Single && o.Button == button)
                        .ToArray();
		    }
        }

        private MenuItem[] FindMenusByText(string text)
        {
            var allMenuItems = FindAllMenuItems();
            var matches = allMenuItems.Where(mi => mi.Text == text);
            return matches.ToArray();
        }

        private List<MenuItem> FindAllMenuItems(List<MenuItem> foundSoFar = null, Menu parent = null)
        {
            foundSoFar = foundSoFar ?? new List<MenuItem>();
            parent = parent ?? _notificationIcon.ContextMenu;
            foreach (var item in parent.MenuItems)
            {
                var menuItem = item as MenuItem;
                if (menuItem == null)
                    continue;   // not sure what else we could find in here?
                foundSoFar.Add(menuItem);
                FindAllMenuItems(foundSoFar, menuItem);
            }
            return foundSoFar;
        }

        public void Init(Icon icon)
        {
            if (_alreadyInitialized)
                throw new TrayIconAlreadyInitializedException();
            _alreadyInitialized = true;
            _icon = icon;
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

        private static MenuItem CreateMenuItemWithText(string withText)
        {
            return new MenuItem()
            {
                Text = withText
            };
        }


    }

    public class TrayIconAlreadyInitializedException : Exception
    {
        public TrayIconAlreadyInitializedException() : base("This instance of the TrayIcon has already been initialized")
        {
        }
    }
}
