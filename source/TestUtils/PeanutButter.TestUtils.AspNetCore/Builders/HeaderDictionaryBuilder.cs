using Microsoft.AspNetCore.Http;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using static PeanutButter.RandomGenerators.RandomValueGen;

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

    public class RandomHeaderDictionaryBuilder : GenericBuilder<RandomHeaderDictionaryBuilder, IHeaderDictionary>
    {
        public override IHeaderDictionary Build()
        {
            return HeaderDictionaryBuilder.BuildRandom();
        }
    }
}