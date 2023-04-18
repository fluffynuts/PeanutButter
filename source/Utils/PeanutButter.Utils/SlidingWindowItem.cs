using System;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Describes a sliding window item
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISlidingWindowItem<out T>
    {
        /// <summary>
        /// The stored value
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// When this value was stored. Inserted values will
        /// have their Created time interpolated from surrounding items
        /// </summary>
        public DateTime Created { get; }
    }

    internal class SlidingWindowItem<T> : ISlidingWindowItem<T>
    {
        public T Value { get; internal set; }
        public DateTime Created { get; internal set; }

        internal SlidingWindowItem(T value)
        {
            Created = DateTime.Now;
            Value = value;
        }
    }
}