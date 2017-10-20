using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable IntroduceOptionalParameters.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace PeanutButter.TrayIcon
{
    /// <summary>
    /// Animator for the Tray Icon
    /// </summary>
    public class TrayIconAnimator
    {
        private readonly TrayIcon _trayIcon;
        private readonly Icon _restStateIcon;
        private readonly Icon[] _animationFrames;
        private string _lastText;
        private bool _busy;
        private Task _animationTask;

        /// <summary>
        /// Constructs an animator
        /// </summary>
        /// <param name="trayIcon">Tray icon to work with</param>
        /// <param name="restStateIcon">Icon to use when not animating</param>
        /// <param name="animationFrames">Frames to use during animation</param>
        public TrayIconAnimator(
            TrayIcon trayIcon, 
            Icon restStateIcon, 
            params Icon[] animationFrames)
        {
            _trayIcon = trayIcon;
            _restStateIcon = restStateIcon;
            _animationFrames = animationFrames;
        }

        /// <summary>
        /// Animate as if busy
        /// </summary>
        public void Busy()
        {
            Busy(null);
        }

        private readonly object _lockObject = new object();
        /// <summary>
        /// Animate as if busy, with provided tooltip text
        /// </summary>
        /// <param name="withText"></param>
        public void Busy(string withText)
        {
            lock (_lockObject)
            {
                if (!SetBusy()) return;
            }
            _lastText = _trayIcon.DefaultTipText;
            _trayIcon.DefaultTipText = withText ?? "busy...";
            _animationTask = new Task(() => {
                                                while (IsBusy())
                                                {
                                                    foreach (var frame in _animationFrames)
                                                    {
                                                        _trayIcon.Icon = frame;
                                                        Thread.Sleep(200);
                                                    }
                                                }
            });
            _animationTask.Start();
        }

        private readonly object _busyLock = new object();
        private bool IsBusy() {
            lock(_busyLock) {
                return _busy;
            }
        }
        private bool SetBusy() {
            lock (_busyLock) {
                if (_busy)
                    return false;
                _busy = true;
                return true;
            }
        }
        private bool SetNotBusy() {
            lock (_busyLock)
            {
                if (!_busy)
                    return false;
                _busy = false;
                return true;
            }
        }

        /// <summary>
        /// Stop animating, as if at rest
        /// </summary>
        public void Rest()
        {
            Rest(null);
        }

        /// <summary>
        /// Stop animating, as if at rest, with specific tooltip text
        /// </summary>
        /// <param name="withText"></param>
        public void Rest(string withText)
        {
            lock (this)
            {
                if (!SetNotBusy())
                    return;
            }
            _trayIcon.DefaultTipText = withText ?? _lastText;
            _animationTask?.Wait();
            _animationTask?.Dispose();
            _trayIcon.Icon = _restStateIcon;
        }
    }
}