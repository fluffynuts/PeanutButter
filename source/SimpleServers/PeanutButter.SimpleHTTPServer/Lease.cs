using System;

namespace PeanutButter.SimpleHTTPServer
{
    /// <summary>
    /// Provides an IDisposable interface over
    /// a borrowed item
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Lease<T> : IDisposable
    {
        /// <summary>
        /// The leased item
        /// </summary>
        public T Item { get; }

        private Action _whenReleased;

        /// <summary>
        /// Lease the item, and run the action when released
        /// via .Dispose()
        /// </summary>
        /// <param name="item"></param>
        /// <param name="whenReleased"></param>
        public Lease(
            T item,
            Action whenReleased
        )
        {
            Item = item;
            _whenReleased = whenReleased;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            var callback = _whenReleased;
            _whenReleased = null;
            callback?.Invoke();
        }
    }
}