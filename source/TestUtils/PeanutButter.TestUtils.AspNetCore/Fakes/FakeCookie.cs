using Microsoft.AspNetCore.Http;

namespace PeanutButter.TestUtils.AspNetCore.Fakes
{
    public class FakeCookie
    {
        public string Key { get; }
        public string Value { get; }
        public CookieOptions Options { get; }

        public FakeCookie(
            string key,
            string value,
            CookieOptions options
        )
        {
            Key = key;
            Value = value;
            Options = options;
        }
    }
}