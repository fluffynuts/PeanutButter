using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Routing;
using System.Web.SessionState;
using PeanutButter.RandomGenerators;

namespace PeanutButter.TestUtils.MVC.Builders
{
    public class FakeHttpContextBuilder 
        : GenericBuilderWithFieldAccess<FakeHttpContextBuilder, FakeHttpContext>
    {
        private IPrincipal _principal;
        private string _url;
        private string _method;
        private NameValueCollection _formParameters;
        private NameValueCollection _queryStringParameters;
        private HttpCookieCollection _cookies;
        private SessionStateItemCollection _sessionItems;
        private NameValueCollection _requestHeaders;

        public override FakeHttpContext ConstructEntity()
        {
            return new FakeHttpContext(
                _principal,
                _formParameters,
                _queryStringParameters,
                _cookies,
                _sessionItems,
                _requestHeaders,
                _url,
                method: _method
            );
        }

        public FakeHttpContextBuilder()
        {
            WithPrincipal(FakePrincipalBuilder.BuildDefault())
                .WithUrl(RandomValueGen.GetRandomHttpUrl())
                .WithFormParameters(NameValueCollectionBuilder.BuildDefault())
                .WithQueryStringParameters(NameValueCollectionBuilder.BuildDefault())
                .WithCookies(HttpCookieCollectionBuilder.BuildDefault())
                .WithSession(SessionStateItemCollectionBuilder.BuildDefault())
                .WithRequestHeaders(NameValueCollectionBuilder.BuildDefault())
                .WithHttpHandlerFor(c => RequestContextBuilder.Create().WithHttpContext(c).Build());
        }

        public FakeHttpContextBuilder WithRequestHeader(
            string header,
            string value)
        {
            return WithField(
                b => b._requestHeaders[header] = value
            );
        }

        public FakeHttpContextBuilder WithRequestHeaders(NameValueCollection headers)
        {
            return WithField(b => b._requestHeaders = headers);
        }

        public FakeHttpContextBuilder WithHttpHandlerFor(
            Func<HttpContextBase, RequestContext> requestContextFactory
        )
        {
            return WithProp(o =>
            {
                o.Handler = MvcHandlerBuilder.Create().WithContext(
                    requestContextFactory(o)
                ).Build();
            });
        }

        public FakeHttpContextBuilder WithSession(
            SessionStateItemCollection sessionItems
        )
        {
            return WithField(b => b._sessionItems = sessionItems);
        }

        public FakeHttpContextBuilder WithCookies(
            HttpCookieCollection cookies
        )
        {
            return WithField(b => b._cookies = cookies);
        }

        public FakeHttpContextBuilder WithQueryStringParameters(
            NameValueCollection queryStringParameters
        )
        {
            return WithField(b => b._queryStringParameters = queryStringParameters);
        }

        public FakeHttpContextBuilder WithFormParameters(
            NameValueCollection formParameters
        )
        {
            return WithField(b => b._formParameters = formParameters);
        }

        public FakeHttpContextBuilder WithPrincipal(IPrincipal principal)
        {
            return WithField(b => b._principal = principal);
        }

        public FakeHttpContextBuilder WithUrl(string url)
        {
            return WithField(b => b._url = url);
        }

        public FakeHttpContextBuilder WithMethod(string method)
        {
            return WithField(b => b._method = method);
        }

        public static HttpContextBase BuildFor(HttpContext context)
        {
            return Create()
                .WithCookies(context.Response.Cookies)
                .WithPrincipal(context.User)
                .WithUrl(context.Request.RawUrl)
                .WithFormParameters(context.Request.Form)
                .WithQueryStringParameters(context.Request.Params)
                .WithSession(CreateSessionStateItemCollectionFor(context.Session))
                .Build();
        }

        private static SessionStateItemCollection CreateSessionStateItemCollectionFor(
            HttpSessionState sessionState
        )
        {
            var result = new SessionStateItemCollection();
            return sessionState?.Keys.AsEnumerable().Aggregate(
                result,
                (acc, cur) => acc.Set(cur, sessionState[cur])
            ) ?? result;
        }
    }

    public static class SessionExtensions
    {
        public static IEnumerable<string> AsEnumerable(
            this NameObjectCollectionBase.KeysCollection keysCollection
        )
        {
            for (var i = 0; i < keysCollection.Count; i++)
                yield return keysCollection[i];
        }

        public static SessionStateItemCollection Set(
            this SessionStateItemCollection collection,
            string key,
            object value
        )
        {
            collection[key] = value;
            return collection;
        }
    }
}
