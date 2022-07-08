using PeanutButter.TestUtils.AspNetCore.Fakes;
using static PeanutButter.RandomGenerators.RandomValueGen;
// ReSharper disable ClassNeverInstantiated.Global

namespace PeanutButter.TestUtils.AspNetCore.Builders
{
    public class HeaderDictionaryBuilder
        : Builder<HeaderDictionaryBuilder, FakeHeaderDictionary>
    {
        public override HeaderDictionaryBuilder Randomize()
        {
            return WithRandomTimes(
                o => o.Add(GetRandomString(4), GetRandomString(4))
            );
        }
    }
}