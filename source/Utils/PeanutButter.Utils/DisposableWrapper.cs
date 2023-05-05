using System;

namespace PeanutButter.Utils
{
    /// <summary>
    /// The delegate type for disposable wrapper event handling
    /// </summary>
    public delegate void DisposableWrapperEventHandler(
        object sender,
        DisposableWrapperEventArgs args
    );

    /// <summary>
    /// The delegate type for disposable wrapper event handling
    /// </summary>
    public delegate void DisposableWrapperErrorEventHandler(
        object sender,
        DisposableWrapperErrorEventArgs args
    );

    /// <summary>
    /// The event arguments raised with every DisposableWrapper event
    /// </summary>
    public class DisposableWrapperEventArgs
    {
        /// <summary>
        /// The name of the event being raised
        /// </summary>
        public string EventName { get; }

        /// <summary>
        /// The disposable in play
        /// </summary>
        public IDisposable Disposable { get; }

        internal DisposableWrapperEventArgs(
            string eventName,
            IDisposable disposable
        )
        {
            EventName = eventName;
            Disposable = disposable;
        }
    }

    /// <summary>
    /// The event args raised when the disposable .Dispose chucks an error
    /// </summary>
    public class DisposableWrapperErrorEventArgs : DisposableWrapperEventArgs
    {
        /// <summary>
        /// The exception thrown during disposal
        /// </summary>
        public Exception Exception { get; }

        internal DisposableWrapperErrorEventArgs(
            string eventName,
            IDisposable disposable,
            Exception exception
        ) : base(eventName, disposable)
        {
            Exception = exception;
        }
    }

    /// <summary>
    /// Provides an eventing wrapper around another disposable item,
    /// raising events before and after disposal and on disposal error
    /// </summary>
    public class DisposableWrapper : IDisposable
    {
        private IDisposable _disposable;

        /// <summary>
        /// Wrap the provided disposable item
        /// </summary>
        /// <param name="disposable"></param>
        public DisposableWrapper(IDisposable disposable)
        {
            _disposable = disposable;
        }

        /// <summary>
        /// Raised before the disposable is disposed
        /// </summary>
        public DisposableWrapperEventHandler BeforeDisposing { get; set; }

        /// <summary>
        /// Raised after the disposable is disposed
        /// </summary>
        public DisposableWrapperEventHandler AfterDisposing { get; set; }

        /// <summary>
        /// Raised when an exception is thrown during disposal
        /// </summary>
        public DisposableWrapperErrorEventHandler OnDisposingError { get; set; }

        /// <inheritdoc />
        public void Dispose()
        {
            TryRaise(BeforeDisposing, nameof(BeforeDisposing));
            try
            {
                _disposable?.Dispose();
                TryRaise(AfterDisposing, nameof(AfterDisposing));
            }
            catch (Exception ex)
            {
                TryRaiseExceptionHandlersFor(ex);
                throw;
            }
            finally
            {
                _disposable = null;
            }
        }

        private void TryRaiseExceptionHandlersFor(
            Exception exception
        )
        {
            TryRun(() =>
            {
                var handler = OnDisposingError;
                handler?.Invoke(
                    this,
                    new DisposableWrapperErrorEventArgs(
                        nameof(OnDisposingError),
                        _disposable,
                        exception
                    )
                );
            });
        }

        private void TryRaise(
            DisposableWrapperEventHandler handler,
            string eventName
        )
        {
            TryRun(() =>
            {
                handler?.Invoke(
                    this,
                    new DisposableWrapperEventArgs(
                        eventName,
                        _disposable
                    )
                );
            });
        }

        private void TryRun(Action action)
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
    }
}