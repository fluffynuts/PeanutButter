using PeanutButter.TestUtils.AspNetCore.Fakes;

namespace PeanutButter.TestUtils.AspNetCore.Builders
{
    public class FakeHeaderDictionaryBuilder
        : Builder<FakeHeaderDictionaryBuilder, FakeHeaderDictionary>
    {
        public static FakeHeaderDictionaryBuilder Create()
        {
            return new();
        }

        public static FakeHeaderDictionary BuildDefault()
        {
            return Create().Build();
        }
    }
}