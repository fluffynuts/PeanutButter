using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable IntroduceOptionalParameters.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace PeanutButter.TrayIcon
{
    public class TrayIconAnimator
    {
        private readonly TrayIcon _trayIcon;
        private readonly Icon _restStateIcon;
        private readonly Icon[] _animationFrames;
        private string _lastText;
        private bool _busy;
        private Task _animationTask;

        public TrayIconAnimator(TrayIcon trayIcon, Icon restStateIcon, params Icon[] animationFrames)
        {
            _trayIcon = trayIcon;
            _restStateIcon = restStateIcon;
            _animationFrames = animationFrames;
        }

        public void Busy()
        {
            Busy(null);
        }

        public void Busy(string withText)
        {
            lock (this)
            {
                if (_busy) return;
                _busy = true;
            }
            _lastText = _trayIcon.DefaultTipText;
            _trayIcon.DefaultTipText = withText ?? "busy...";
            _animationTask = new Task(() => {
                                                while (_busy)
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

        public void Rest()
        {
            Rest(null);
        }

        public void Rest(string withText)
        {
            lock (this)
            {
                if (!_busy) return;
                _busy = false;
            }
            _trayIcon.DefaultTipText = withText ?? _lastText;
            _animationTask.Wait();
            _animationTask.Dispose();
            _trayIcon.Icon = _restStateIcon;
        }
    }
}