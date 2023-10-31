using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// Provides a mechanism to dispose of other disposables when it is disposed.
    /// This allows for flattening out nested using() blocks with an outer AutoDisposer
    /// which takes care of disposing registered items (in reverse order) when it is
    /// disposed
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        class AutoDisposer: IDisposable
    {
        private readonly Action<IDisposable> _beforeDisposing;
        private readonly List<IDisposable> _toDispose = new();

        /// <summary>
        /// When enabled, will background &amp; parallelize disposal
        /// </summary>
        public bool ThreadedDisposal { get; set; } = false;

        /// <summary>
        /// When enabled, disposal happens in the background instead
        /// of halting the caller on the .Dispose() line
        /// </summary>
        public bool BackgroundDisposal { get; set; } = false;

        /// <summary>
        /// Constructs a new AutoDisposer
        /// </summary>
        /// <param name="toDispose">Params array of objects implementing IDisposable which the AutoDisposer will dispose of when it it disposed</param>
        public AutoDisposer(params IDisposable[] toDispose)
        {
            Add(toDispose);
        }

        /// <summary>
        /// Adds zero or more IDisposable objects to the list to be disposed when this AutoDisposer is disposed
        /// </summary>
        /// <param name="toDispose">Params array of objects to watch. Objects are disposed in reverse order.</param>
        public void Add(params IDisposable[] toDispose)
        {
            _toDispose.AddRange(toDispose);
        }

        /// <summary>
        /// Constructs a new AutoDisposer with an action to run, per-item,
        /// before disposing items
        /// </summary>
        /// <param name="beforeDisposing"></param>
        public AutoDisposer(Action<IDisposable> beforeDisposing)
        {
            _beforeDisposing = beforeDisposing;
        }

        /// <summary>
        /// Adds a single IDisposable object to the disposable list and returns that object.
        /// Use this to make your code flow better, eg:
        /// var someDisposable = autoDisposer.Add(new SomeDisposable());
        /// </summary>
        /// <param name="toDispose">IDisposable to dispose of at a later date</param>
        /// <typeparam name="T">The type of the IDisposable to add</typeparam>
        /// <returns>The item added to the auto-disposing collection</returns>
        public T Add<T>(T toDispose) where T : IDisposable
        {
            _toDispose.Add(toDispose);
            return toDispose;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            lock (this)
            {
                var toDispose = _toDispose.ToArray().Reverse().ToArray();
                _toDispose.Clear();
                if (ThreadedDisposal)
                {
                    CleanupInParallel(toDispose);
                }
                else
                {
                    CleanupInSerial(toDispose);
                }
            }
        }

        private void CleanupInSerial(IDisposable[] toDispose)
        {
            var t = new Thread(() =>
            {
                foreach (var disposable in toDispose)
                {
                    SafelyDispose(disposable);
                }
            });
            t.Start();
            if (BackgroundDisposal)
            {
                return;
            }
            t.Join();
        }

        private void CleanupInParallel(IDisposable[] toDispose)
        {
            var t = new Thread(() =>
            {
                Parallel.ForEach(toDispose, d =>
                {
                    TryDo(() => _beforeDisposing?.Invoke(d));
                    TryDo(d.Dispose);
                });
            });
            t.Start();
            if (BackgroundDisposal)
            {
                return;
            }
            t.Join();
        }

        private static void TryDo(Action action)
        {
            try
            {
                action();
            }
            catch
            {
                // suppress
            }
        }

        private static void SafelyDispose(IDisposable disposable)
        {
            try
            {
                disposable.Dispose();
            }
            catch
            {
                // ignored
            }
        }
        
        /// <summary>
        /// Dispose of the item right now, rather than later
        /// </summary>
        /// <param name="disposable"></param>
        public void DisposeNow(IDisposable disposable)
        {
            _toDispose.Remove(disposable);
            SafelyDispose(disposable);
        }
    }
}
