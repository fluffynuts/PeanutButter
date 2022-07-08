using Microsoft.AspNetCore.Http;

namespace PeanutButter.TestUtils.AspNetCore.Fakes
{
    /// <summary>
    /// Implements a fake cookie holder
    /// </summary>
    public class FakeCookie: IFake
    {
        /// <summary>
        /// The name of the cookie
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The value of the cookie
        /// </summary>
        public string Value { get; }
        /// <summary>
        /// The options for the cookie
        /// </summary>
        public CookieOptions Options { get; }

        /// <summary>
        /// Constructs the cookie container
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public FakeCookie(
            string name,
            string value,
            CookieOptions options
        )
        {
            Name = name;
            Value = value;
            Options = options;
        }
    }
}