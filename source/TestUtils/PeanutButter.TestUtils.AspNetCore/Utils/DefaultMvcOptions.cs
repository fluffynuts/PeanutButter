using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace PeanutButter.TestUtils.AspNetCore.Utils;

/// <summary>
/// Provides a null-implementation for IOptions&lt;MvcOptions&gt;
/// </summary>
public class DefaultMvcOptions : DefaultOptions<MvcOptions>
{
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public class DefaultOptions<T> : DefaultOptionsWithFactory<T> 
    where T: class, new()
{
    /// <inheritdoc />
    public DefaultOptions(): base(() => new T())
    {
    }
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public class DefaultOptionsWithFactory<T> : IOptions<T> where T : class
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