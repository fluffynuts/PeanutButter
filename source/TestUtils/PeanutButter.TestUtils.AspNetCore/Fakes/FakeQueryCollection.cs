using System.Linq;
using Microsoft.AspNetCore.Http;

namespace PeanutButter.TestUtils.AspNetCore.Fakes
{
    public class FakeQueryCollection : StringValueMap, IQueryCollection
    {
        public FakeQueryCollection()
        {
        }

        public FakeQueryCollection(string queryString)
        {
            queryString ??= "";
            queryString = queryString.Trim();
            if (queryString == "")
            {
                return;
            }

            if (queryString[0] == '?')
            {
                queryString = queryString.Substring(1);
            }

            var parts = queryString.Split('&');
            foreach (var part in parts)
            {
                var sub = part.Split('=');
                var key = sub[0];
                var value = string.Join("=", sub.Skip(1));
                Store[key] = value;
            }
        }
    }
}