using System;
using System.Collections;
using System.Collections.Generic;

namespace PeanutButter.Utils.Dictionaries;

internal interface IOrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
{
}

[Obsolete("OrderedDictionary is incomplete: there is no guarantee of key order yet")]
internal class OrderedDictionary<TKey, TValue>: IOrderedDictionary<TKey, TValue>
{
    private readonly Dictionary<TKey, TValue> _actual = new();
        
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return _actual.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        _actual.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return _actual.TryGetValue(item.Key, out var value) &&
            AreEqual(item.Value, value);
    }

    private bool AreEqual(TValue left, TValue right)
    {
        if (left is null && right is null)
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }
        return left.Equals(right);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        foreach (var key in Keys)
        {
            array[arrayIndex++] = new KeyValuePair<TKey, TValue>(key, this[key]);
        }
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        return TryGetValue(item.Key, out var value) &&
            AreEqual(value, item.Value) && 
            Remove(item.Key);
    }

    public int Count => _actual.Count;
    public bool IsReadOnly => false;
    public bool ContainsKey(TKey key)
    {
        return _actual.ContainsKey(key);
    }

    public void Add(TKey key, TValue value)
    {
        _actual.Add(key, value);
    }

    public bool Remove(TKey key)
    {
        return _actual.Remove(key);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        return _actual.TryGetValue(key, out value);
    }

    public TValue this[TKey key]
    {
        get => _actual[key];
        set => _actual[key] = value;
    }

    public ICollection<TKey> Keys => _actual.Keys;
    public ICollection<TValue> Values => _actual.Values;
}