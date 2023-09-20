using System;
using System.Collections;
using System.Reflection;
using System.Web;

namespace PeanutButter.TestUtils.MVC
{
    // refactored from: http://bigjimindc.blogspot.co.za/2007/07/ms-kb928365-aspnet-requestheadersadd.html
    public static class HttpRequestExtensions
    {
        private static readonly BindingFlags PrivateInstanceInvoke =
            BindingFlags.InvokeMethod |
            BindingFlags.NonPublic |
            BindingFlags.Instance;

        public static void SetHeader(this HttpRequest request, string name, string value)
        {
            var item = new ArrayList
            {
                value
            };
            var headers = request.Headers;
            var type = headers.GetType();
            Invoke(type, "MakeReadWrite", headers);
            Invoke(type, "InvalidateCachedArrays", headers);
            Invoke(type, "BaseAdd", headers, new object[] {name, item});
            Invoke(type, "MakeReadOnly", headers);
        }

        private static void Invoke(
            Type type,
            string method,
            object target,
            object[] args = null
        )
        {
            type.InvokeMember(method, PrivateInstanceInvoke, null, target, args);
        }
    }
}
