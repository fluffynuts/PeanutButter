using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace PeanutButter.TestUtils.AspNetCore.Fakes
{
    
    internal static class CookieUtil
    {
        /// <summary>
        /// Sets the Cookie header on the provided request from
        /// the collection defining cookie values
        /// </summary>
        /// <param name="cookies"></param>
        /// <param name="request"></param>
        /// <param name="overwrite"></param>
        public static void GenerateCookieHeader(
            IEnumerable<KeyValuePair<string, string>> cookies,
            HttpRequest request, 
            bool overwrite
        )
        {
            var items = new List<string>();
            if (!overwrite)
            {
                var current = request.Headers[HEADER].FirstOrDefault();
                if (current is not null)
                {
                    items.Add(current);
                }
            }

            foreach (var cookie in cookies)
            {
                items.Add($"{WebUtility.UrlEncode(cookie.Key)}={WebUtility.UrlEncode(cookie.Value)}");
            }
            var header = string.Join($"{DELIMITER} ", items);
            if (string.IsNullOrWhiteSpace(header))
            {
                request.Headers.Remove(HEADER);
            }
            else
            {
                request.Headers[HEADER] = header;
            }
        }
        
        public const string HEADER = "Cookie";
        public const string DELIMITER = ";";
    }
}