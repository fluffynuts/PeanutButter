using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace PeanutButter.TestUtils.MVC
{
    public static class NameValueCollectionExtensions
    {
        public static string AsQueryString(this NameValueCollection collection)
        {
            return collection.AllKeys.Aggregate("",
                                                (acc, cur) => $"{acc}&{Enc(cur)}={Enc(collection[(string) cur])}"
            ).TrimStart(new[] {'&'});

            string Enc(string s)
            {
                return HttpUtility.UrlEncode(s);
            }
        }
    }
}
