using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using NSubstitute;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TestUtils.MVC
{
    public class FakeHttpContext : HttpContextBase
    {
        private IPrincipal _principal;
        private readonly NameValueCollection _formParams;
        private readonly NameValueCollection _queryStringParams;
        private readonly HttpCookieCollection _cookies;
        private readonly SessionStateItemCollection _sessionItems;
        private readonly NameValueCollection _requestHeaders;
        private readonly FakeHttpResponse _response;
        private readonly string _url;
        private readonly string _contentType;
        private readonly string _requestContents;
        private HttpRequestBase _request;
        private HttpApplication _application;
        private string _method;
        private string _applicationPath;
        private IDictionary _items;

        public FakeHttpContext(
            string url = "https://www.yumbi.com") : this(
            principal:null,
            formParams: new NameValueCollection(),
            queryStringParams: new NameValueCollection(),
            cookies: new HttpCookieCollection(),
            sessionItems: new SessionStateItemCollection(),
            requestHeaders: new NameValueCollection(),
            url: url
            )
        {
        }

        public FakeHttpContext(
            IPrincipal principal,
            NameValueCollection formParams = null,
            NameValueCollection queryStringParams = null,
            HttpCookieCollection cookies = null,
            SessionStateItemCollection sessionItems = null,
            NameValueCollection requestHeaders = null,
            string url = null,
            string contentType = "text/html",
            string requestContents = "",
            string method = "GET",
            string applicationPath = "/"
        )
        {
            _principal = principal;
            _formParams = formParams ?? new NameValueCollection();
            _queryStringParams = queryStringParams ?? new NameValueCollection();
            _cookies = cookies ?? new HttpCookieCollection();
            _sessionItems = sessionItems ?? new SessionStateItemCollection();
            _requestHeaders = requestHeaders ?? new NameValueCollection();
            _url = url ?? GetRandomHttpUrl();
            _contentType = contentType;
            _requestContents = requestContents;
            _method = method;
            _response = new FakeHttpResponse();
            _applicationPath = applicationPath;
            _items = new Dictionary<string, object>();
        }

        public override HttpRequestBase Request =>
            (_request ?? (_request = FakeHttpRequest()));

        private FakeHttpRequest FakeHttpRequest()
        {
            return new FakeHttpRequest(
                _formParams,
                _queryStringParams,
                _cookies,
                _requestHeaders,
                _url,
                _contentType,
                _requestContents,
                _method,
                _applicationPath
            );
        }

        public override IPrincipal User
        {
            get => _principal;
            set => _principal = value;
        }

        public override IHttpHandler Handler { get; set; }

        public override HttpSessionStateBase Session =>
            new FakeHttpSessionState(_sessionItems);

        public override HttpResponseBase Response => _response;

        public override HttpApplication ApplicationInstance =>
            _application ?? (_application = new FakeHttpApplication(this));

        public void SetupMvcHandler(RouteData routeData)
        {
            Handler = new MvcHandler(
                new RequestContext(
                    this,
                    routeData)
                );
        }

        public override object GetService(Type serviceType)
        {
            return Substitute.For(
                new Type[] {serviceType},
                new object[] { });
        }

        public override IDictionary Items
        {
            get => _items;
        }

        public void SetItemValue(
            string key,
            string value)
        {
            _items[key] = value;
        }

    }
}

