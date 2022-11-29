using System;
using System.Threading;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Provides an easy way to cache on a single value generator
    /// </summary>
    public class SingleItemCache<T>
    {
        private readonly Func<T> _generator;
        private readonly long _timeToLive;
        private readonly object _lockObject = new();
        private T _cachedValue;

        private long _freshUntilMs = 0;

        /// <summary>
        /// The value for the generator, or a cached value,
        /// if available and still fresh enough
        /// </summary>
        public T Value => RetrieveValue();

        private T RetrieveValue()
        {
            lock (_lockObject)
            {
                if (_freshUntilMs > DateTime.Now.Ticks)
                {
                    return _cachedValue;
                }

                _cachedValue = _generator();
                _freshUntilMs = DateTime.Now.Ticks + _timeToLive;
            }

            return _cachedValue;
        }

        /// <summary>
        /// Creates the tiny cache
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="timeToLive"></param>
        /// <exception cref="NotImplementedException"></exception>
        public SingleItemCache(
            Func<T> generator,
            TimeSpan timeToLive
        )
        {
            _generator = generator;
            _timeToLive = timeToLive.Ticks;
        }

        /// <summary>
        /// Invalidates the cache such that the next call will definitely
        /// regenerate the value
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Invalidate()
        {
            Interlocked.Exchange(ref _freshUntilMs, 0);
        }
    }
}