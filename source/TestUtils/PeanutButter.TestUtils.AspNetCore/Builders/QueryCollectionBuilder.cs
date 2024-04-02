using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using Microsoft.AspNetCore.Http;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using PeanutButter.Utils;

// ReSharper disable UnusedType.Global

// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.TestUtils.AspNetCore.Builders;

/// <summary>
/// Alias to QueryCollectionBuilder
/// </summary>
public class QueryBuilder : QueryCollectionBuilder
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="existingUri"></param>
    /// <returns></returns>
    public static QueryCollectionBuilder Create(Uri existingUri)
    {
        var dict = HttpUtility.ParseQueryString(existingUri.Query)
            .ToDictionary();
        return QueryCollectionBuilder.Create()
            .WithParameters(dict);
    }
}

/// <summary>
/// Builds query collections
/// </summary>
public class QueryCollectionBuilder
    : StringMapDerivativeBuilder<QueryCollectionBuilder, IQueryCollection, FakeQueryCollection>
{
    /// <inheritdoc />
    public override QueryCollectionBuilder Randomize()
    {
        return WithRandomItems();
    }

    /// <summary>
    /// Establishes some random parameters in the collection
    /// </summary>
    /// <returns></returns>
    public QueryCollectionBuilder WithRandomParameters()
    {
        return WithRandomItems();
    }

    /// <summary>
    /// Sets a parameter in the collection. Non-string
    /// values are allowed for this method, but they will
    /// be stringified via interpolation, so make sure
    /// that the result of .ToString() is valid on
    /// what you're passing in here...
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public QueryCollectionBuilder WithParameter<T>(
        string key,
        T value
    )
    {
        return SetItem(key, value);
    }

    /// <summary>
    /// Bulk-sets parameters on the query. Existing parameters
    /// will be overwritten.
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public QueryCollectionBuilder WithParameters(
        IDictionary<string, string> parameters
    )
    {
        if (parameters is null)
        {
            throw new ArgumentNullException(nameof(parameters));
        }

        foreach (var kvp in parameters)
        {
            WithParameter(kvp.Key, kvp.Value);
        }

        return this;
    }

    /// <summary>
    /// Bulk-sets parameters on the query. Existing parameters
    /// will be overwritten.
    /// </summary>
    /// <param name="existingParameters"></param>
    /// <exception cref="NotImplementedException"></exception>
    public QueryCollectionBuilder WithParameters(
        NameValueCollection existingParameters
    )
    {
        return WithParameters(existingParameters?.ToDictionary());
    }
}