using System.Collections;
using System.Collections.Generic;

namespace PeanutButter.TestUtils.AspNetCore;

/// <summary>
/// Provides a collection of string maps
/// </summary>
public class StringMap : IEnumerable<KeyValuePair<string, string>>
{
    /// <summary>
    /// Clears the store
    /// </summary>
    public void Clear()
    {
        _store.Clear();
    }

    private readonly Dictionary<string, string> _store = new();

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        return _store.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Returns the count of the store
    /// </summary>
    public int Count => _store.Count;

    /// <summary>
    /// Returns all keys in the store
    /// </summary>
    public ICollection<string> Keys => _store.Keys;

    /// <summary>
    /// Returns true if the key is stored
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool ContainsKey(string key)
    {
        return _store.ContainsKey(key);
    }

    /// <summary>
    /// Attempts to get the value associated with the provided key
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetValue(string key, out string value)
    {
        return _store.TryGetValue(key, out value);
    }

    /// <summary>
    /// Indexes into the store
    /// </summary>
    /// <param name="key"></param>
    public string this[string key]
    {
        get => _store[key];
        set => _store[key] = value;
    }
}