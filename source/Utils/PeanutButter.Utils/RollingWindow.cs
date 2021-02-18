using System;
using System.Collections;
using System.Collections.Generic;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Provides a rolling window of data to the given size
    /// Disposing the RollingWindow immediately releases all
    /// references to items within the inner collection and
    /// renders the instance unusable from then on.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRollingWindow<T> : IEnumerable<T>, IDisposable
    {
        /// <summary>
        /// Get or set the maximum size for this window. If you set the MaxSize
        /// after construction to a value smaller than before and the RollingWindow
        /// is already at capacity, extraneous items will be trimmed.
        /// </summary>
        long MaxSize { get; set; }

        /// <summary>
        /// Add an item to the window, will discard any old items which exceed
        /// the configured window length
        /// </summary>
        /// <param name="item"></param>
        void Add(T item);
    }

    /// <inheritdoc />
    public class RollingWindow<T> : IRollingWindow<T>
    {
        /// <inheritdoc />
        public long MaxSize
        {
            get => RunLocked(() => _maxSize);
            set => RunLocked(() =>
            {
                _maxSize = value;
                TrimUnlocked(_maxSize);
            });
        }

        private long _maxSize;
        private Queue<T> _queue;
        private Queue<T> Queue => _queue ?? throw new ObjectDisposedException(nameof(RollingWindow<T>));

        /// <summary>
        /// Create a rolling window of max size maxSize
        /// </summary>
        /// <param name="maxSize"></param>
        /// <exception cref="ArgumentException"></exception>
        public RollingWindow(long maxSize)
        {
            if (maxSize < 1)
            {
                throw new ArgumentException($"{nameof(RollingWindow<T>)}: size must be > 0", nameof(maxSize));
            }

            _maxSize = maxSize;
            _queue = new Queue<T>();
        }

        /// <inheritdoc />
        public void Add(T item)
        {
            RunLocked(() =>
            {
                Queue.Enqueue(item);
                TrimUnlocked(_maxSize);
            });
        }

        private T[] Snapshot()
        {
            return RunLocked(
                () => Queue.ToArray()
            );
        }

        /// <inheritdoc />
        public void Dispose()
        {
            RunLocked(() =>
            {
                TrimUnlocked(0);
                _queue = null;
            });
        }

        private TResult RunLocked<TResult>(Func<TResult> toRun)
        {
            lock (Queue)
            {
                return toRun();
            }
        }

        private void TrimUnlocked(long toSize)
        {
            var queue = Queue;
            while (queue.Count > toSize)
            {
                queue.Dequeue();
            }
        }

        private void RunLocked(Action toRun)
        {
            lock (Queue)
            {
                toRun();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new RollingWindowSnapshotEnumerator<T>(
                Snapshot()
            );
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    internal class RollingWindowSnapshotEnumerator<T> : IEnumerator<T>
    {
        private T[] _snapshot;
        private int _idx;

        public RollingWindowSnapshotEnumerator(T[] snapshot)
        {
            Reset();
            _snapshot = snapshot;
        }

        public bool MoveNext()
        {
            _idx++;
            return _idx < _snapshot.Length;
        }

        public void Reset()
        {
            _idx = -1;
        }

        public T Current => _snapshot[_idx];

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            // nothing to do
        }
    }
}