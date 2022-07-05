using Microsoft.AspNetCore.Http;
using PeanutButter.TestUtils.AspNetCore.Fakes;

namespace PeanutButter.TestUtils.AspNetCore.Builders
{
    public class FakeRequestCookieCollectionBuilder
        : Builder<FakeRequestCookieCollectionBuilder, FakeRequestCookieCollection>
    {
        public static FakeRequestCookieCollectionBuilder Create()
        {
            return new();
        }

        public static IRequestCookieCollection BuildDefault()
        {
            return Create().Build();
        }
    }
}