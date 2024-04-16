#if BUILD_PEANUTBUTTER_INTERNAL
using Imported.PeanutButter.TestUtils.AspNetCore.Fakes;
namespace Imported.PeanutButter.TestUtils.AspNetCore.Builders;
#else
using PeanutButter.TestUtils.AspNetCore.Fakes;
namespace PeanutButter.TestUtils.AspNetCore.Builders;
#endif

/// <summary>
/// Provides a base builder for fake collections derived off of StringValueMap
/// </summary>
/// <typeparam name="TBuilder"></typeparam>
/// <typeparam name="TInterface"></typeparam>
/// <typeparam name="TConcrete"></typeparam>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    abstract class StringMapDerivativeBuilder<TBuilder, TInterface, TConcrete>
    : RandomizableBuilder<TBuilder, TInterface>
    where TBuilder : RandomizableBuilder<TBuilder, TInterface>, new()
    where TConcrete : class, TInterface, IFake, ICanBeIndexedBy<string>, new()
{
    /// <inheritdoc />
    protected override TInterface ConstructEntity()
    {
        return new TConcrete();
    }

    /// <summary>
    /// adds random items to the collection
    /// </summary>
    /// <returns></returns>
    protected virtual TBuilder WithRandomItems()
    {
        return With(
            o =>
            {
                var target = o.As<TConcrete>();
                var howMany = RandomGenerators.RandomValueGen.GetRandomInt(2);
                for (var i = 0; i < howMany; i++)
                {
                    target[RandomGenerators.RandomValueGen.GetRandomString(5)] =
                        RandomGenerators.RandomValueGen.GetRandomString();
                }
            }
        );
    }

    /// <summary>
    /// Adds an item to the collection - call this by
    /// a more specifically-named method on your derivative
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    protected virtual TBuilder SetItem<T>(string key, T value)
    {
        return With(
            o =>
            {
                o.As<TConcrete>()[key] = value as string ?? $"{value}";
            }
        );
    }
}