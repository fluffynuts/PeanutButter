using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http.Features;

namespace PeanutButter.TestUtils.AspNetCore.Fakes
{
    public class FakeFeatureCollection : IFeatureCollection
    {
        private Dictionary<Type, object> _store = new();

        public IEnumerator<KeyValuePair<Type, object>> GetEnumerator()
        {
            return _store.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public TFeature Get<TFeature>()
        {
            return _store.TryGetValue(typeof(TFeature), out var result)
                ? (TFeature) result
                : default;
        }

        public void Set<TFeature>(TFeature instance)
        {
            _store[typeof(TFeature)] = instance;
        }

        public bool IsReadOnly => false;
        public int Revision { get; set; }

        public object this[Type key]
        {
            get => _store.TryGetValue(key, out var result)
                ? result
                : default;
            set => _store[key] = value;
        }
    }
}