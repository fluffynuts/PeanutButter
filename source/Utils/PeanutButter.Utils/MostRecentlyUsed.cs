using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Holds a list of most-recently-used items, up to
    /// the provided capacity. New items displace the older ones,
    /// and re-adding the same item  will reposition that item at the top of
    /// the list.
    ///
    /// Object equality testing is via .GetHashCode(), for
    /// performance reasons - so primitives will "just work", but your
    /// own types will require you to implement .GetHashCode()
    /// (and .Equals() for completeness' sake) so that the internal
    /// dictionary can properly index by the object)
    ///
    /// MostRecentlyUsedList&lt;T&gt; is not thread-safe:
    /// if you plan on accessing this (read or write) from multiple
    /// threads, you must lock it yourself.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MostRecentlyUsedList<T> : IEnumerable<T>
    {
        /// <summary>
        /// The capacity of this list. Once this is reached, adding
        /// new items will evict older ones.
        /// </summary>
        public int Capacity { get; }

        private Dictionary<T, long> _items = new();

        /// <summary>
        /// Holds a list of most-recently-used items, up to
        /// the provided capacity. New items displace the older ones,
        /// and re-adding the same item (test is via .Equals) will
        /// reposition that item at the top of the list
        /// </summary>
        public MostRecentlyUsedList(int capacity)
        {
            Capacity = capacity;
        }

        /// <summary>
        /// Adds an item to the collection. If the capacity of the collection
        /// is exceeded, the last-used value will be evicted.
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            _items[item] = DateTime.Now.Ticks;
            if (_items.Count <= Capacity)
            {
                return;
            }

            var oldest = _items.OrderBy(kvp => kvp.Value)
                .First();
            _items.Remove(oldest.Key);
        }

        /// <summary>
        /// Tests if the provided item is in the list.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            return _items.ContainsKey(item);
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return new MostRecentlyUsedEnumerator<T>(this);
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        internal Dictionary<T, long> Items => _items;
    }

    internal class MostRecentlyUsedEnumerator<T> : IEnumerator<T>
    {
        private readonly IOrderedEnumerable<KeyValuePair<T, long>> _snapshot;
        private readonly IEnumerator<KeyValuePair<T, long>> _snapshotEnumerator;

        public MostRecentlyUsedEnumerator(
            MostRecentlyUsedList<T> list
        )
        {
            _snapshot = list.Items
                .OrderByDescending(kvp => kvp.Value);
            _snapshotEnumerator = _snapshot.GetEnumerator();
        }

        public void Dispose()
        {
            _snapshotEnumerator.Dispose();
        }

        public bool MoveNext()
        {
            return _snapshotEnumerator.MoveNext();
        }

        public void Reset()
        {
            _snapshotEnumerator.Reset();
        }

        public T Current {
            get => _snapshotEnumerator.Current.Key;
            set => throw new InvalidOperationException("Cannot set the current value on an enumerator");
        }
        object IEnumerator.Current => Current;
    }
}