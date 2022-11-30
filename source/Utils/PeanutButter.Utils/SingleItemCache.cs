using System;
using System.Threading;
// ReSharper disable UnusedMember.Global
// ReSharper disable TypeParameterCanBeVariant

namespace PeanutButter.Utils
{
    /// <summary>
    /// SingleItemCache provides a light, fast, caching wrapper
    /// around a function to generate a value with a provided TTL
    /// </summary>
    public interface ISingleItemCache<T>
    {
        /// <summary>
        /// The value for the generator, or a cached value,
        /// if available and still fresh enough
        /// </summary>
        T Value { get; }

        /// <summary>
        /// Expose the generator (might be useful for testing purposes)
        /// </summary>
        Func<T> Generator { get; }

        /// <summary>
        /// Expose the provided TimeToLive (might be useful for testing purposes)
        /// </summary>
        TimeSpan TimeToLive { get; }

        /// <summary>
        /// Invalidates the cache such that the next call will definitely
        /// regenerate the value
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        void Invalidate();
    }

    /// <summary>
    /// SingleItemCache provides a light, fast, caching wrapper
    /// around a function to generate a value with a provided TTL
    /// </summary>
    public class SingleItemCache<T>
        : ISingleItemCache<T>
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
        
        /// <summary>
        /// Expose the generator (might be useful for testing purposes)
        /// </summary>
        public Func<T> Generator => _generator;

        /// <summary>
        /// Expose the provided TimeToLive (might be useful for testing purposes)
        /// </summary>
        public TimeSpan TimeToLive { get; }

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
        /// SingleItemCache provides a light, fast, caching wrapper
        /// around a function to generate a value with a provided TTL
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
            TimeToLive = timeToLive;
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