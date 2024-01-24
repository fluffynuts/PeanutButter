using System;
using System.Collections;
using System.Collections.Generic;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils.Dictionaries;
#else
namespace PeanutButter.Utils.Dictionaries;
#endif
/// <summary>
/// Actions which can be performed on a ValidatingDictionary.
/// The action will be passed into the validator so that the
/// consumer can do tricks like allowing adding certain keys
/// but not removing them.
/// </summary>
public enum Mutation
{
    /// <summary>
    /// New item is being added
    /// </summary>
    Create,

    /// <summary>
    /// Existing item is being updated
    /// </summary>
    Update,

    /// <summary>
    /// Existing item is being removed -
    /// this is only raised if the item
    /// would actually be removed
    /// </summary>
    Remove,

    /// <summary>
    /// All items are being cleared
    /// </summary>
    Clear,
}

/// <summary>
/// Provides a dictionary implementation which
/// allows for a validation action for keys, values
/// and pairs
/// </summary>
public class ValidatingDictionary<TKey, TValue> : IDictionary<TKey, TValue>
{
    private readonly Dictionary<TKey, TValue> _actual;
    private readonly Action<IDictionary<TKey, TValue>, TKey, TValue, Mutation> _validator;

    /// <summary>
    /// Construct a validating dictionary with a validator
    /// and use the default equality comparer for key lookups
    /// </summary>
    /// <param name="validator"></param>
    public ValidatingDictionary(
        Action<IDictionary<TKey, TValue>, TKey, TValue, Mutation> validator
    ) : this(validator, equalityComparer: null)
    {
    }

    /// <summary>
    /// Construct the validating dictionary with a validator
    /// and an equality comparer to use for key lookups
    /// </summary>
    /// <param name="validator"></param>
    /// <param name="equalityComparer"></param>
    public ValidatingDictionary(
        Action<IDictionary<TKey, TValue>, TKey, TValue, Mutation> validator,
        IEqualityComparer<TKey> equalityComparer
    )
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _actual = equalityComparer is null
            ? new Dictionary<TKey, TValue>()
            : new Dictionary<TKey, TValue>(equalityComparer);
    }

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return _actual.GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc />
    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    /// <inheritdoc />
    public void Clear()
    {
        IfValidThen(
            default,
            default,
            Mutation.Clear,
            () => _actual.Clear()
        );
    }

    /// <inheritdoc />
    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return _actual.TryGetValue(item.Key, out var value) &&
            value.Equals(item.Value);
    }

    /// <inheritdoc />
    public void CopyTo(
        KeyValuePair<TKey, TValue>[] array,
        int arrayIndex
    )
    {
        foreach (var kvp in _actual)
        {
            array[arrayIndex++] = kvp;
        }
    }

    /// <inheritdoc />
    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        if (!_actual.TryGetValue(item.Key, out var value))
        {
            return false;
        }

        if (value.Equals(item.Value))
        {
            _actual.Remove(item.Key);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public int Count => _actual.Count;

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <inheritdoc />
    public bool ContainsKey(TKey key)
    {
        return _actual.ContainsKey(key);
    }

    /// <inheritdoc />
    public void Add(TKey key, TValue value)
    {
        IfValidThen(
            key,
            value,
            Mutation.Create,
            () => _actual.Add(key, value)
        );
    }

    /// <inheritdoc />
    public bool Remove(TKey key)
    {
        if (!TryGetValue(key, out var value))
        {
            return false;
        }

        _validator(this, key, value, Mutation.Remove);
        return _actual.Remove(key);
    }

    /// <inheritdoc />
    public bool TryGetValue(TKey key, out TValue value)
    {
        return _actual.TryGetValue(key, out value);
    }

    /// <inheritdoc />
    public TValue this[TKey key]
    {
        get => _actual[key];
        set => IfValidThen(key, value, Mutation.Create, () => _actual[key] = value);
    }

    private void IfValidThen(
        TKey key,
        TValue value,
        Mutation mutation,
        Action runIfValid
    )
    {
        if (mutation == Mutation.Create && ContainsKey(key))
        {
            mutation = Mutation.Update;
        }

        _validator(this, key, value, mutation);
        runIfValid();
    }

    /// <inheritdoc />
    public ICollection<TKey> Keys => _actual.Keys;

    /// <inheritdoc />
    public ICollection<TValue> Values => _actual.Values;
}