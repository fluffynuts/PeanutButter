using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http.Features;

namespace PeanutButter.TestUtils.AspNetCore.Fakes
{
    /// <summary>
    /// Implements a fake feature collection
    /// </summary>
    public class FakeFeatureCollection : IFeatureCollection, IFake
    {
        private readonly Dictionary<Type, object> _store = new();

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<Type, object>> GetEnumerator()
        {
            return _store.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public TFeature Get<TFeature>()
        {
            return _store.TryGetValue(typeof(TFeature), out var result)
                ? (TFeature) result
                : default;
        }

        /// <inheritdoc />
        public void Set<TFeature>(TFeature instance)
        {
            _store[typeof(TFeature)] = instance;
        }

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public int Revision { get; set; }

        /// <inheritdoc />
        public object this[Type key]
        {
            get => _store.TryGetValue(key, out var result)
                ? result
                : default;
            set => _store[key] = value;
        }
    }
}