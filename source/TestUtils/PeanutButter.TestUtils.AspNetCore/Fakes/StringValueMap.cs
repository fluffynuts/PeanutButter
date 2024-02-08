using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using PeanutButter.Utils.Dictionaries;
// ReSharper disable PublicConstructorInAbstractClass

namespace PeanutButter.TestUtils.AspNetCore.Fakes;

/// <summary>
/// Provides a string-to-stringvalue map
/// </summary>
public abstract class StringValueMap
    : IEnumerable, ICanBeIndexedBy<string>
{
    /// <summary>
    /// delegate raised when this collection changes
    /// </summary>
    public delegate void ChangedDelegate(object sender, StringValueMapChangedEventArgs args);

    /// <summary>
    /// Event raised when this collection changes
    /// </summary>
    public ChangedDelegate OnChanged;

    // public delegate void TempDbDisposedEventHandler(object sender, TempDbDisposedEventArgs args);
    private readonly IEqualityComparer<string> _equalityComparer;

    /// <summary>
    /// Create a string-value map around the provided store
    /// </summary>
    /// <param name="store"></param>
    public StringValueMap(IDictionary<string, StringValues> store)
    {
        _store = store;
    }

    /// <summary>
    /// Create a blank string-value map
    /// </summary>
    public StringValueMap() : this(StringComparer.OrdinalIgnoreCase)
    {
    }

    /// <summary>
    /// Create a blank string-value map with the provided equality comparer for keys
    /// </summary>
    /// <param name="equalityComparer"></param>
    public StringValueMap(
        IEqualityComparer<string> equalityComparer
    )
    {
        _equalityComparer = equalityComparer;
    }

    private IDictionary<string, StringValues> CreateDefaultStore()
    {
        return new DefaultDictionary<string, StringValues>(
            () => new StringValues(),
            _equalityComparer
        );
    }

    /// <summary>
    /// The store for the values
    /// </summary>
    protected IDictionary<string, StringValues> Store
    {
        get => _store ??= CreateDefaultStore();
        set => _store = value ?? CreateDefaultStore();
    }

    private IDictionary<string, StringValues> _store;

    /// <summary>
    /// Get an enumerator for the items in the store
    /// </summary>
    /// <returns></returns>
    public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator()
    {
        return Store.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Returns true if the key is contained in the store
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool ContainsKey(string key)
    {
        return Store.ContainsKey(key);
    }

    /// <summary>
    /// Attempts to get the value associated with the provided key
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetValue(string key, out StringValues value)
    {
        return Store.TryGetValue(key, out value);
    }

    private void NotifyChanged()
    {
        var handlers = OnChanged;
        if (handlers is null)
        {
            return;
        }

        handlers(this, new StringValueMapChangedEventArgs(Store));
    }

    private T NotifyChangedAfter<T>(Func<T> func)
    {
        var result = func();
        NotifyChanged();
        return result;
    }

    private void NotifyChangedAfter(Action action)
    {
        action();
        NotifyChanged();
    }

    /// <summary>
    /// Adds a key-value-pair to the store
    /// </summary>
    /// <param name="item"></param>
    public void Add(KeyValuePair<string, StringValues> item)
    {
        NotifyChangedAfter(() => Store.Add(item));
    }

    /// <summary>
    /// Clears the store
    /// </summary>
    public void Clear()
    {
        NotifyChangedAfter(() => Store.Clear());
    }

    /// <summary>
    /// Tests if the key-value-pair is in the store
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Contains(KeyValuePair<string, StringValues> item)
    {
        return Store.Contains(item);
    }

    /// <summary>
    /// Copies the contents to the provided array
    /// </summary>
    /// <param name="array"></param>
    /// <param name="arrayIndex"></param>
    public void CopyTo(KeyValuePair<string, StringValues>[] array, int arrayIndex)
    {
        Store.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Removes an item from the store
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Remove(KeyValuePair<string, StringValues> item)
    {
        return NotifyChangedAfter(() => Store.Remove(item));
    }

    /// <summary>
    /// Adds an item to the store
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Add(string key, StringValues value)
    {
        NotifyChangedAfter(
            () => Add(new KeyValuePair<string, StringValues>(key, value))
        );
    }

    /// <summary>
    /// Removes the item associated with the provided key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool Remove(string key)
    {
        return NotifyChangedAfter(() => Store.Remove(key));
    }

    /// <summary>
    /// Returns all known values
    /// </summary>
    public ICollection<StringValues> Values => Store.Values;

    /// <summary>
    /// Returns read-only status of the store
    /// </summary>
    public bool IsReadOnly => Store.IsReadOnly;

    /// <summary>
    /// Returns the count of the store
    /// </summary>
    public int Count => Store.Count;

    /// <summary>
    /// Returns all keys in the store
    /// </summary>
    public ICollection<string> Keys => Store.Keys;

    private void Set(string key, string value)
    {
        NotifyChangedAfter(() => Store[key] = value);
    }

    /// <summary>
    /// Indexes into the store
    /// </summary>
    /// <param name="key"></param>
    public StringValues this[string key]
    {
        get => Store[key];
        set => Set(key, value);
    }
}