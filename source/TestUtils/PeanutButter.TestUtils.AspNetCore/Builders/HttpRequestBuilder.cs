using System;
using System.IO;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Http;
using PeanutButter.TestUtils.AspNetCore.Fakes;

namespace PeanutButter.TestUtils.AspNetCore.Builders
{
    public class HttpRequestBuilder : Builder<HttpRequestBuilder, HttpRequest>
    {
        protected override HttpRequest ConstructEntity()
        {
            return new FakeHttpRequest();
        }

        public override HttpRequestBuilder Randomize()
        {
            // TODO
            throw new NotImplementedException();
        }
        
        public HttpRequestBuilder(): base(Actualize)
        {
            WithForm(FormBuilder.BuildDefault())
                .WithMethod(HttpMethod.Get)
                .WithScheme("http")
                .WithPath("/")
                .WithHost("localhost")
                .WithPort(80)
                .WithHeaders(HeaderDictionaryBuilder.BuildDefault())
                .WithCookies(RequestCookieCollectionBuilder.BuildDefault())
                .WithHttpContext(() =>
                    HttpContextBuilder
                        .CreateWithRequestFactory(
                            () => CurrentEntity
                        )
                        .Build()
                );
        }

        private static void Actualize(HttpRequest built)
        {
            Warn(built.HttpContext is null, "no HttpContext set");
            Warn(built.HttpContext?.Request is null, "no HttpContext.Request set");
        }

        private static void Warn(
            bool condition,
            string message
        )
        {
        }

        public HttpRequestBuilder WithBody(string body)
        {
            return WithBody(Encoding.UTF8.GetBytes(body));
        }

        public HttpRequestBuilder WithBody(byte[] body)
        {
            return WithBody(new MemoryStream(body));
        }

        public HttpRequestBuilder WithBody(Stream body)
        {
            return With(
                o => o.Body = body
            );
        }

        public HttpRequestBuilder WithCookie(string name, string value)
        {
            return With(
                o =>
                {
                    var cookies = o.Cookies as FakeRequestCookieCollection
                        ?? throw new InvalidImplementationException(o.Cookies, nameof(o.Cookies));
                    cookies[name] = value;
                }
            );
        }

        public HttpRequestBuilder WithCookies(IRequestCookieCollection cookies)
        {
            return With(
                o => o.Cookies = cookies
            );
        }

        public HttpRequestBuilder WithHeader(
            string header,
            string value
        )
        {
            return With(
                o => o.Headers[header] = value
            );
        }

        public HttpRequestBuilder WithHeaders(IHeaderDictionary headers)
        {
            return With<FakeHttpRequest>(
                o => o.SetHeaders(headers)
            );
        }

        public HttpRequestBuilder WithQuery(IQueryCollection query)
        {
            return With(
                o => o.Query = query
            );
        }

        public HttpRequestBuilder WithPath(string path)
        {
            return With(
                o => o.Path = new PathString(SanitisePath(path))
            );
        }

        private static string SanitisePath(string path)
        {
            path ??= "";
            return !path.StartsWith("/")
                ? $"/{path}"
                : path;
        }

        public HttpRequestBuilder WithBasePath(string basePath)
        {
            return With(
                o => o.PathBase = new PathString(SanitisePath(basePath))
            );
        }

        public HttpRequestBuilder WithHost(HostString host)
        {
            return With(
                o => o.Host = host
            );
        }

        public HttpRequestBuilder WithPort(int port)
        {
            return With(
                o => o.Host = new HostString(o.Host.Host, port)
            );
        }

        public HttpRequestBuilder WithHost(string host)
        {
            return With(
                o => o.Host = o.Host.Port is null
                    ? new HostString(host)
                    : new HostString(host, o.Host.Port.Value)
            );
        }

        public HttpRequestBuilder WithQueryString(
            string queryString
        )
        {
            return WithQueryString(new QueryString(queryString));
        }

        public HttpRequestBuilder WithQueryString(
            QueryString queryString
        )
        {
            return With(
                o => o.QueryString = queryString
            );
        }

        public HttpRequestBuilder WithScheme(string scheme)
        {
            return With(
                o => o.Scheme = scheme
            );
        }

        public HttpRequestBuilder WithMethod(HttpMethod method)
        {
            return WithMethod(method.ToString());
        }

        public HttpRequestBuilder WithMethod(string method)
        {
            return With(o => o.Method = (method ?? "get").ToUpper());
        }

        public HttpRequestBuilder WithHttpContext(
            HttpContext context
        )
        {
            return WithHttpContext(() => context);
        }

        public HttpRequestBuilder WithHttpContext(
            Func<HttpContext> factory
        )
        {
            return With<FakeHttpRequest>(
                o => o.SetContextAccessor(factory)
            );
        }

        public HttpRequestBuilder WithForm(IFormCollection formCollection)
        {
            return With(
                o => o.Form = formCollection
            );
        }
    }
}