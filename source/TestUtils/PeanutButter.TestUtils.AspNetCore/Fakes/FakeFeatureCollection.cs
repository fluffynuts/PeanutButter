using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http.Features;
// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable UnusedAutoPropertyAccessor.Global

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore.Fakes;
#else
namespace PeanutButter.TestUtils.AspNetCore.Fakes;
#endif

/// <summary>
/// Implements a fake feature collection
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class FakeFeatureCollection : IFeatureCollection, IFake
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
        if (_store.TryGetValue(key, out var service))
        {
            return service;
        }

        return _factories.TryGetValue(key, out var factory)
            ? factory()
            : default;
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