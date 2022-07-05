using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace PeanutButter.TestUtils.AspNetCore;

public abstract class StringValueMap
    : IEnumerable
{
    private readonly IEqualityComparer<string> _equalityComparer;

    public StringValueMap(IDictionary<string, StringValues> store)
    {
        _store = store;
    }

    public StringValueMap() : this(StringComparer.Ordinal)
    {
    }

    public StringValueMap(
        IEqualityComparer<string> equalityComparer
    )
    {
        _equalityComparer = equalityComparer;
    }

    private IDictionary<string, StringValues> CreateDefaultStore()
    {
        return new Dictionary<string, StringValues>(_equalityComparer);
    }

    protected IDictionary<string, StringValues> Store
    {
        get => _store ??= CreateDefaultStore();
        set => _store = value ?? CreateDefaultStore();
    }

    private IDictionary<string, StringValues> _store;

    public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator()
    {
        return Store.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool ContainsKey(string key)
    {
        return Store.ContainsKey(key);
    }

    public bool TryGetValue(string key, out StringValues value)
    {
        return Store.TryGetValue(key, out value);
    }

    public void Add(KeyValuePair<string, StringValues> item)
    {
        Store.Add(item);
    }

    public void Clear()
    {
        Store.Clear();
    }

    public bool Contains(KeyValuePair<string, StringValues> item)
    {
        return Store.Contains(item);
    }

    public void CopyTo(KeyValuePair<string, StringValues>[] array, int arrayIndex)
    {
        Store.CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<string, StringValues> item)
    {
        return Store.Remove(item);
    }

    public void Add(string key, StringValues value)
    {
        Add(new KeyValuePair<string, StringValues>(key, value));
    }

    public bool Remove(string key)
    {
        return Store.Remove(key);
    }

    public ICollection<StringValues> Values => Store.Values;

    public bool IsReadOnly => Store.IsReadOnly;
    public int Count => Store.Count;
    public ICollection<string> Keys => Store.Keys;

    public StringValues this[string key]
    {
        get => Store[key];
        set => Store[key] = value;
    }
}