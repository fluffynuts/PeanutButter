using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PeanutButter.Utils
{
    internal class EmptyEnumerator<T>
        : IEnumerator<T>
    {
        public bool MoveNext()
        {
            return false;
        }

        public void Reset()
        {
        }

        public T Current => default;

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }
    }

    internal class InfiniteEnumerator<T> : IEnumerator<T>
    {
        private readonly IEnumerable<T> _collection;
        private IEnumerator<T> _currentEnumerator;

        public InfiniteEnumerator(IEnumerable<T> collection)
        {
            _collection = collection ?? throw new ArgumentException(nameof(collection));
            ResetCollectionEnumerator();
        }

        private void ResetCollectionEnumerator()
        {
            _currentEnumerator?.Dispose();
            _currentEnumerator = _collection.GetEnumerator();
        }

        public bool MoveNext()
        {
            return _currentEnumerator.MoveNext()
                || ResetAndMoveNext();
        }

        private bool ResetAndMoveNext()
        {
            ResetCollectionEnumerator();
            return _currentEnumerator.MoveNext();
        }

        public void Reset()
        {
            ResetCollectionEnumerator();
        }

        public T Current => _currentEnumerator.Current;

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            _currentEnumerator?.Dispose();
        }
    }

    /// <summary>
    /// Represents a list which circles back on itself such
    /// that enumerating over it produces an unending series.
    /// EG: if it was created with the numbers [ 1, 2, 3 ],
    /// then enumeration would yield [ 1, 2, 3, 1, 2, 3, 1 ... ]
    /// For all operations except enumeration and indexing,
    /// the collection will behave as an infinitely repeating
    /// series. For obvious reasons, CopyTo will copy the
    /// internal, limited collection.
    /// </summary>
    public class CircularList<T> : IList<T>
    {
        private readonly List<T> _store;

        /// <inheritdoc />
        public CircularList() : this(Enumerable.Empty<T>())
        {
        }

        /// <summary>
        /// Initializes the CircularList with some items
        /// </summary>
        /// <param name="items"></param>
        public CircularList(IEnumerable<T> items)
        {
            _store = new List<T>(items);
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            if (_store.Count == 0)
            {
                return new EmptyEnumerator<T>();
            }

            return new InfiniteEnumerator<T>(_store.ToArray());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public void Add(T item)
        {
            _store.Add(item);
        }

        /// <inheritdoc />
        public void Clear()
        {
            _store.Clear();
        }

        /// <inheritdoc />
        public bool Contains(T item)
        {
            return _store.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(T[] array, int arrayIndex)
        {
            _store.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public bool Remove(T item)
        {
            return _store.Remove(item);
        }

        /// <inheritdoc />
        public int Count => _store.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public int IndexOf(T item)
        {
            return _store.IndexOf(item);
        }

        /// <inheritdoc />
        public void Insert(int index, T item)
        {
            _store.Insert(index, item);
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            _store.RemoveAt(Modulo(index));
        }

        /// <inheritdoc />
        public T this[int index]
        {
            get => _store[Modulo(index)];
            set => _store[Modulo(index)] = value;
        }

        private int Modulo(int index)
        {
            return _store.Count == 0
                ? throw new ArgumentOutOfRangeException()
                : index % _store.Count;
        }
    }
}