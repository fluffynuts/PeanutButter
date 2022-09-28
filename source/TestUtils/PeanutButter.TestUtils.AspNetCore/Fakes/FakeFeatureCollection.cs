using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http.Features;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PeanutButter.TestUtils.AspNetCore.Fakes;

/// <summary>
/// Implements a fake feature collection
/// </summary>
public class FakeFeatureCollection : IFeatureCollection, IFake
{
    private readonly Dictionary<Type, object> _store = new();
    private readonly Dictionary<Type, Func<object>> _factories = new();

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
        return (TFeature)ResolveService(typeof(TFeature));
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
        get { return ResolveService(key); }
        set
        {
            if (value is Func<object>)
            {
                throw new NotSupportedException(
                    $"Cannot set a factory via the indexer"
                );
            }

            _store[key] = value;
        }
    }

    private object ResolveService(Type key)
    {
        if (_store.ContainsKey(key))
        {
            return _store[key];
        }

        if (_factories.ContainsKey(key))
        {
            return _factories[key]();
        }

        return default;
    }

    /// <summary>
    /// Sets a factory for the service
    /// </summary>
    /// <param name="func"></param>
    /// <typeparam name="TFeature"></typeparam>
    /// <exception cref="NotImplementedException"></exception>
    public void SetFactory<TFeature>(Func<TFeature> func)
    {
        SetFactory(typeof(TFeature), () => func());
    }

    /// <summary>
    /// Sets a factory for the service
    /// </summary>
    /// <param name="type"></param>
    /// <param name="factory"></param>
    public void SetFactory(Type type, Func<object> factory)
    {
        _store.Remove(type);
        _factories[type] = factory;
    }
}