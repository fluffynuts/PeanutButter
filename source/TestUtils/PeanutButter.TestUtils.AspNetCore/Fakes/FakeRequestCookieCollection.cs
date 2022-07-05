using Microsoft.AspNetCore.Http;

namespace PeanutButter.TestUtils.AspNetCore.Fakes
{
    public class FakeRequestCookieCollection : StringMap, IRequestCookieCollection
    {
    }
}