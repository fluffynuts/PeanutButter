using System;
using System.Threading;

// ReSharper disable UnusedMember.Global
// ReSharper disable TypeParameterCanBeVariant

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// SingleItemCache provides a light, fast, caching wrapper
    /// around a function to generate a value with a provided TTL
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        interface ISingleItemCache<T>
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
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        class SingleItemCache<T>
        : ISingleItemCache<T>
    {
        private readonly Func<T> _generator;
        private readonly Func<bool> _cacheInvalidator;
        private readonly long _timeToLive;
        private T _cachedValue;
        private long _lastFetched;

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

        /// <summary>
        /// When provided via the alternative constructor
        /// </summary>
        public Func<bool> CacheInvalidator => _cacheInvalidator;

        private T RetrieveValue()
        {
            return _cacheInvalidator is null
                ? RetrieveValueWithTimeToLive()
                : RetrieveValueWithCacheInvalidator();
        }

        private T RetrieveValueWithCacheInvalidator()
        {
            var now = DateTime.Now.Ticks;
            var lastFetched = Interlocked.Exchange(ref _lastFetched, now);
            var shouldGenerate = lastFetched == 0 || CacheInvalidator();
            return shouldGenerate
                ? _cachedValue = Generator()
                : _cachedValue;
        }

        private T RetrieveValueWithTimeToLive()
        {
            var now = DateTime.Now.Ticks;
            var lastFetched = Interlocked.Exchange(ref _lastFetched, now);
            if (now - lastFetched < _timeToLive)
            {
                return _cachedValue;
            }

            return _cachedValue = _generator();
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
        /// SingleItemCache provides a light, fast, caching wrapper
        /// around a function to generate a value with a provided
        /// function to test if the cache should be invalidated
        /// before the each read
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="cacheInvalidator"></param>
        public SingleItemCache(
            Func<T> generator,
            Func<bool> cacheInvalidator
        )
        {
            _generator = generator;
            _cacheInvalidator = cacheInvalidator;
            TimeToLive = TimeSpan.MaxValue;
        }

        /// <summary>
        /// Invalidates the cache such that the next call will definitely
        /// regenerate the value
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Invalidate()
        {
            Interlocked.Exchange(ref _lastFetched, 0);
        }
    }
}