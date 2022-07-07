using PeanutButter.TestUtils.AspNetCore.Fakes;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TestUtils.AspNetCore.Builders
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class RequestCookieCollectionBuilder
        : Builder<RequestCookieCollectionBuilder, FakeRequestCookieCollection>
    {
        public override RequestCookieCollectionBuilder Randomize()
        {
            return WithRandomTimes(
                o => o[GetRandomString(4)] = GetRandomString(4, 20)
            );
        }
    }
}