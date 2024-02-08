using Microsoft.AspNetCore.Http;
using PeanutButter.TestUtils.AspNetCore.Fakes;
// ReSharper disable UnusedType.Global

// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.TestUtils.AspNetCore.Builders;

/// <summary>
/// Alias to QueryCollectionBuilder
/// </summary>
public class QueryBuilder : QueryCollectionBuilder;

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
}
