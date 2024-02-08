using Microsoft.AspNetCore.Http;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using static PeanutButter.RandomGenerators.RandomValueGen;
// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.TestUtils.AspNetCore.Builders;

/// <summary>
/// Builds query collections
/// </summary>
public class QueryCollectionBuilder : RandomizableBuilder<QueryCollectionBuilder, IQueryCollection>
{
    /// <inheritdoc />
    protected override IQueryCollection ConstructEntity()
    {
        return new FakeQueryCollection();
    }

    /// <inheritdoc />
    public override QueryCollectionBuilder Randomize()
    {
        return WithRandomParameters();
    }

    /// <summary>
    /// Establishes some random parameters in the collection
    /// </summary>
    /// <returns></returns>
    public QueryCollectionBuilder WithRandomParameters()
    {
        return With(
            o =>
            {
                var target = o.As<FakeQueryCollection>();

                var howMany = GetRandomInt(2);
                for (var i = 0; i < howMany; i++)
                {
                    target[GetRandomString(5)] = GetRandomString();
                }
            }
        );
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
        return With(o =>
        {
            o.As<FakeQueryCollection>()[key] = $"{value}";
        });
    }
}