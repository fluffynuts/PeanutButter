using System;
using System.Collections.Generic;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Provides a mechanism to dispose of other disposables when it is disposed.
    /// This allows for flattening out nested using() blocks with an outer AutoDisposer
    /// which takes care of disposing registered items (in reverse order) when it is
    /// disposed
    /// </summary>
    public class AutoDisposer: IDisposable
    {
        private readonly List<IDisposable> _toDispose;

        /// <summary>
        /// Constructs a new AutoDisposer
        /// </summary>
        /// <param name="toDispose">Params array of objects implementing IDisposable which the AutoDisposer will dispose of when it it disposed</param>
        public AutoDisposer(params IDisposable[] toDispose)
        {
            _toDispose = new List<IDisposable>();
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
                _toDispose.Reverse();
                foreach (var disposable in _toDispose)
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
                _toDispose.Clear();
            }
        }
    }
}
