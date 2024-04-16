using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

// ReSharper disable MemberCanBeProtected.Global

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore.Utils;
#else
namespace PeanutButter.TestUtils.AspNetCore.Utils;
#endif

/// <summary>
/// Provides a null-implementation for IOptions&lt;MvcOptions&gt;
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class DefaultMvcOptions : DefaultOptions<MvcOptions>
{
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class DefaultOptions<T> : DefaultOptionsWithFactory<T>
    where T : class, new()
{
    /// <inheritdoc />
    public DefaultOptions() : base(() => new T())
    {
    }
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class DefaultOptionsWithFactory<T> : IOptions<T> where T : class
{
    /// <inheritdoc />
    public T Value { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="factory"></param>
    public DefaultOptionsWithFactory(Func<T> factory)
    {
        Value = factory();
    }
}