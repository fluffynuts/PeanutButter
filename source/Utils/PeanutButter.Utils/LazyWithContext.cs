using System;
using System.Threading;
using System.Threading.Tasks;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// Provides a mechanism for lazy evaluation with a provider context
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        interface ILazyWithContext<TValue>
    {
        /// <summary>
        /// The lazily-evaluated value
        /// </summary>
        TValue Value { get; }
    }

    /// <inheritdoc />
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        class LazyWithContext<TContext, TValue> : ILazyWithContext<TValue>
    {
        /// <summary>
        /// The lazily-evaluated value
        /// </summary>
        public TValue Value => ResolveValue();

        private readonly TContext _context;
        private TValue _cached;
        private bool _evaluated;
        private readonly SemaphoreSlim _lock = new(1);
        private readonly Func<TContext, TValue> _synchronousValueFactory;
        private readonly Func<TContext, Task<TValue>> _asyncValueFactory;

        /// <summary>
        /// Synchronous Lazy constructor
        /// </summary>
        /// <param name="context">host provider (for `this` context)</param>
        /// <param name="valueFactory">method to run to get the value</param>
        public LazyWithContext(
            TContext context,
            Func<TContext, TValue> valueFactory)
        {
            _context = context;
            _synchronousValueFactory = valueFactory ??
                throw new ArgumentNullException(nameof(valueFactory));
        }

        /// <summary>
        /// Asynchronous Lazy constructor
        /// </summary>
        /// <param name="context">host provider (for `this` context)</param>
        /// <param name="valueFactory">method to run to get the value</param>
        public LazyWithContext(
            TContext context,
            Func<TContext, Task<TValue>> valueFactory)
        {
            _context = context;
            _asyncValueFactory = valueFactory
                ?? throw new ArgumentNullException(nameof(valueFactory));
        }

        private TValue ResolveValue()
        {
            if (_evaluated)
            {
                return _cached;
            }

            using (new AutoLocker(_lock))
            {
                // re-check the cached value (only lock if necessary, since it's relatively expensive)
                if (_evaluated)
                {
                    return _cached;
                }

                _cached = _synchronousValueFactory == null
                    ? ResolveValueAsync()
                    : ResolveValueSync();
                _evaluated = true;
                return _cached;
            }
        }

        private TValue ResolveValueAsync()
        {
            var task = _asyncValueFactory.Invoke(_context);
            task.ConfigureAwait(false);
            return task.Result;
        }

        private TValue ResolveValueSync()
        {
            return _synchronousValueFactory.Invoke(_context);
        }
    }
}