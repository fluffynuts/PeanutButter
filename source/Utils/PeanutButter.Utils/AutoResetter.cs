using System;

namespace
#if BUILD_PEANUTBUTTER_INTERNAL
    Imported.PeanutButter.Utils
#else
    PeanutButter.Utils
#endif
{
    /// <summary>
    /// Provides a mechanism to run code on construction and disposal,
    /// irrespective of exception handling
    /// Use this, for example, to set up and tear down state required for
    /// a test -- your constructionAction is called immediately upon construction
    /// and the using() pattern guarantees that your disposalAction is called at
    /// disposal, even if your test fails.
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
    public
#endif
        class AutoResetter : IDisposable
    {
        private readonly object _lock = new object();
        private Action _disposalAction;

        /// <summary>
        /// Constructs a new AutoResetter and immediately runs the constructionAction
        /// </summary>
        /// <param name="constructionAction">Action to run at construction time</param>
        /// <param name="disposalAction">Action to run at disposal time</param>
        public AutoResetter(Action constructionAction, Action disposalAction)
        {
            constructionAction();
            _disposalAction = disposalAction;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            lock (_lock)
            {
                try
                {
                    _disposalAction?.Invoke();
                }
                finally
                {
                    _disposalAction = null;
                }
            }
        }
    }
}