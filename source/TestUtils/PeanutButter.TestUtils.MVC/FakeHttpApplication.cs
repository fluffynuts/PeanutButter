using System.IO;
using System.Reflection;
using System.Web;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.MVC
{
    public class FakeHttpApplication : HttpApplication
    {

        public FakeHttpApplication(HttpContextBase baseContext)
        {
            var request = baseContext.Request;
            var httpRequest = new HttpRequest(
                request.FilePath,
                request.Url?.AbsoluteUri ?? "http://its-a-mystery.com",
                request.QueryString.AsQueryString()
            );
            baseContext.Request.Headers.AllKeys.ForEach(
                k =>
                {
                    httpRequest.SetHeader(k, baseContext.Request.Headers[k]);
                });
            SetContext(new HttpContext(
                           httpRequest,
                           new HttpResponse(new StringWriter())
                       ));
        }

        private void SetContext(object httpContext)
        {
            var fieldInfo = typeof(HttpApplication).GetField("_context", BindingFlags.Instance | BindingFlags.NonPublic);
            fieldInfo?.SetValue(this, httpContext);
        }
    }
}
