using System;
using System.Collections;
using System.Collections.Generic;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils.Dictionaries;
#else
namespace PeanutButter.Utils.Dictionaries;
#endif

/// <summary>
/// Provides access to an underlying dictionary with the keys held
/// intact and the values passed through a transform
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public class TransformingDictionary<TKey, TValue> : IDictionary<TKey, TValue>
{
    private readonly Func<KeyValuePair<TKey, TValue>, TValue> _mutator;
    private readonly IDictionary<TKey, TValue> _underlyingData;

    /// <summary>
    /// Construct the transforming dictionary without an external
    /// data store - this may be useful for, eg, sanitising data
    /// </summary>
    /// <param name="mutator"></param>
    public TransformingDictionary(
        Func<KeyValuePair<TKey, TValue>, TValue> mutator
    ) : this(mutator, new Dictionary<TKey, TValue>())
    {
    }

    /// <summary>
    /// Construct the TransformingDictionary with the underlying
    /// data
    /// </summary>
    /// <param name="mutator"></param>
    /// <param name="underlyingData"></param>
    public TransformingDictionary(
        Func<KeyValuePair<TKey, TValue>, TValue> mutator,
        IDictionary<TKey, TValue> underlyingData
    )
    {
        _mutator = mutator ?? throw new ArgumentNullException(
            nameof(mutator)
        );
        _underlyingData = underlyingData ?? throw new ArgumentNullException(
            nameof(underlyingData)
        );
    }

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return new ValueMutatingEnumerator<TKey, TValue>(
            _mutator,
            _underlyingData
        );
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc />
    public void Add(KeyValuePair<TKey, TValue> item)
    {
        _underlyingData.Add(item);
    }

    /// <inheritdoc />
    public void Clear()
    {
        _underlyingData.Clear();
    }

    /// <inheritdoc />
    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return _underlyingData.Contains(item);
    }

    /// <inheritdoc />
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        foreach (var item in _underlyingData)
        {
            array[arrayIndex++] = new KeyValuePair<TKey, TValue>(
                item.Key,
                _mutator(item)
            );
        }
    }

    /// <inheritdoc />
    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        return _underlyingData.Remove(item);
    }

    /// <inheritdoc />
    public int Count => _underlyingData.Count;

    /// <inheritdoc />
    public bool IsReadOnly => _underlyingData.IsReadOnly;

    /// <inheritdoc />
    public void Add(TKey key, TValue value)
    {
        _underlyingData.Add(key, value);
    }

    /// <inheritdoc />
    public bool ContainsKey(TKey key)
    {
        return _underlyingData.ContainsKey(key);
    }

    /// <inheritdoc />
    public bool Remove(TKey key)
    {
        return _underlyingData.Remove(key);
    }

    /// <inheritdoc />
    public bool TryGetValue(TKey key, out TValue value)
    {
        if (_underlyingData.TryGetValue(key, out var actual))
        {
            value = _mutator(new KeyValuePair<TKey, TValue>(key, actual));
            return true;
        }

        value = default;
        return false;
    }

    /// <inheritdoc />
    public TValue this[TKey key]
    {
        get => _mutator(new KeyValuePair<TKey, TValue>(key, _underlyingData[key]));
        set => _underlyingData[key] = value;
    }

    /// <inheritdoc />
    public ICollection<TKey> Keys => _underlyingData.Keys;

    /// <inheritdoc />
    public ICollection<TValue> Values
    {
        get
        {
            var result = new List<TValue>();
            foreach (var item in _underlyingData)
            {
                result.Add(
                    _mutator(item)
                );
            }

            return result;
        }
    }
}

internal class ValueMutatingEnumerator<TKey, TValue>
    : IEnumerator<KeyValuePair<TKey, TValue>>
{
    private readonly Func<KeyValuePair<TKey, TValue>, TValue> _mutator;
    private readonly IEnumerator<KeyValuePair<TKey, TValue>> _underlyingDataEnumerator;

    public ValueMutatingEnumerator(
        Func<KeyValuePair<TKey, TValue>, TValue> mutator,
        IDictionary<TKey, TValue> underlyingData
    )
    {
        _mutator = mutator;
        _underlyingDataEnumerator = underlyingData.GetEnumerator();
    }

    public void Dispose()
    {
        _underlyingDataEnumerator.Dispose();
    }

    public bool MoveNext()
    {
        return _underlyingDataEnumerator.MoveNext();
    }

    public void Reset()
    {
        _underlyingDataEnumerator.Reset();
    }

    public KeyValuePair<TKey, TValue> Current
    {
        get => GenerateMutationForCurrentValue();
    }

    private KeyValuePair<TKey, TValue> GenerateMutationForCurrentValue()
    {
        return new KeyValuePair<TKey, TValue>(
            _underlyingDataEnumerator.Current.Key,
            _mutator(_underlyingDataEnumerator.Current)
        );
    }

    object IEnumerator.Current => Current;
}