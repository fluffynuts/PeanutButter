using System.Web;
using PeanutButter.RandomGenerators;

namespace PeanutButter.TestUtils.MVC.Builders
{
    public class HttpCookieCollectionBuilder
        : GenericBuilder<HttpCookieCollectionBuilder, HttpCookieCollection>
    {
        public HttpCookieCollectionBuilder WithCookie(
            string name,
            string value
        )
        {
            return WithCookie(new HttpCookie(name, value));
        }

        public HttpCookieCollectionBuilder WithCookie(
            HttpCookie cookie
        )
        {
            return WithProp(o => o.Add(cookie));
        }
    }
}
