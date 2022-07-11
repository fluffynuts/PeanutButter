using PeanutButter.TestUtils.AspNetCore.Fakes;
using static PeanutButter.RandomGenerators.RandomValueGen;

// ReSharper disable ClassNeverInstantiated.Global

namespace PeanutButter.TestUtils.AspNetCore.Builders;

/// <summary>
/// Constructs a header dictionary
/// </summary>
public class HeaderDictionaryBuilder
    : RandomizableBuilder<HeaderDictionaryBuilder, FakeHeaderDictionary>
{
    /// <summary>
    /// Randomizes the header collection
    /// </summary>
    /// <returns></returns>
    public override HeaderDictionaryBuilder Randomize()
    {
        return WithRandomTimes(
            o => o.Add(GetRandomString(4), GetRandomString(4))
        );
    }

    /// <summary>
    /// Sets a header in the collection (overwrites any existing header with the same name)
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public HeaderDictionaryBuilder WithHeader(
        string name,
        string value
    )
    {
        return With(
            o => o[name] = value
        );
    }
}