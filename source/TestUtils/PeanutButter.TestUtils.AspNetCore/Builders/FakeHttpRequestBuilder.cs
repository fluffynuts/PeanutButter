using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Http;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TestUtils.AspNetCore.Builders
{
    internal static class Run
    {
        private static HashSet<Type> Seen = new();
        private static object _lock = new();
        
        internal static void Once<T>(
            Action toRun
        )
        {
            lock (_lock)
            {
                if (Seen.Contains(typeof(T)))
                {
                    return;
                }

                Seen.Add(typeof(T));
            }
            toRun();
        }
    }

    public class FakeHttpRequestBuilder : Builder<FakeHttpRequestBuilder, FakeHttpRequest>
    {
        static FakeHttpRequestBuilder()
        {
            InstallRandomGenerators();
        }

        public static void InstallRandomGenerators()
        {
            Run.Once<FakeHttpRequestBuilder>(() =>
            {
                InstallRandomGenerator<HostString>(
                    () => new HostString(
                        GetRandomHostname(),
                        GetRandomFrom(CommonPorts)
                    )
                );
                InstallRandomGenerator<PathString>(
                    () => new PathString(GetRandomPath())
                );
                InstallRandomGenerator<QueryString>(
                    () => new QueryString(GetRandomUrlQuery())
                );
                // TODO: add random generator for FakeHttpRequest
            });
        }

        private static readonly int[] CommonPorts =
        {
            80,
            443,
            5000,
            8000,
            8080
        };

        public static FakeHttpRequestBuilder Create()
        {
            return new FakeHttpRequestBuilder();
        }

        public static FakeHttpRequest BuildDefault()
        {
            return Create().Build();
        }

        public FakeHttpRequestBuilder()
        {
            WithForm(FakeFormBuilder.BuildDefault())
                .WithMethod(HttpMethod.Get)
                .WithScheme("http")
                .WithPath(GetRandomAbsolutePath())
                .WithHost(GetRandom<HostString>())
                .WithHeaders(FakeHeaderDictionaryBuilder.BuildDefault())
                .WithCookies(FakeRequestCookieCollectionBuilder.BuildDefault())
                .WithHttpContext(() =>
                    FakeHttpContextBuilder
                        .CreateWithRequestFactory(
                            () => CurrentEntity
                        )
                        .Build()
                );
        }

        public FakeHttpRequestBuilder WithBody(string body)
        {
            return WithBody(Encoding.UTF8.GetBytes(body));
        }

        public FakeHttpRequestBuilder WithBody(byte[] body)
        {
            return WithBody(new MemoryStream(body));
        }

        public FakeHttpRequestBuilder WithBody(Stream body)
        {
            return With(
                o => o.Body = body
            );
        }

        public FakeHttpRequestBuilder WithCookie(string name, string value)
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

        public FakeHttpRequestBuilder WithCookies(IRequestCookieCollection cookies)
        {
            return With(
                o => o.Cookies = cookies
            );
        }

        public FakeHttpRequestBuilder WithHeader(
            string header,
            string value
        )
        {
            return With(
                o => o.Headers[header] = value
            );
        }

        public FakeHttpRequestBuilder WithHeaders(IHeaderDictionary headers)
        {
            return With(
                o => o.SetHeaders(headers)
            );
        }

        public FakeHttpRequestBuilder WithQuery(IQueryCollection query)
        {
            return With(
                o => o.Query = query
            );
        }

        public FakeHttpRequestBuilder WithPath(string path)
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

        public FakeHttpRequestBuilder WithBasePath(string basePath)
        {
            return With(
                o => o.PathBase = new PathString(SanitisePath(basePath))
            );
        }

        public FakeHttpRequestBuilder WithHost(HostString host)
        {
            return With(
                o => o.Host = host
            );
        }

        public FakeHttpRequestBuilder WithHost(string host)
        {
            return With(
                o => o.Host = o.Host.Port is null
                    ? new HostString(host)
                    : new HostString(host, o.Host.Port.Value)
            );
        }

        public FakeHttpRequestBuilder WithQueryString(
            string queryString
        )
        {
            return WithQueryString(new QueryString(queryString));
        }

        public FakeHttpRequestBuilder WithQueryString(
            QueryString queryString
        )
        {
            return With(
                o => o.QueryString = queryString
            );
        }

        public FakeHttpRequestBuilder WithScheme(string scheme)
        {
            return With(
                o => o.Scheme = scheme
            );
        }

        public FakeHttpRequestBuilder WithMethod(HttpMethod method)
        {
            return WithMethod(method.ToString());
        }

        public FakeHttpRequestBuilder WithMethod(string method)
        {
            return With(o => o.Method = (method ?? "get").ToUpper());
        }

        public FakeHttpRequestBuilder WithHttpContext(
            HttpContext context
        )
        {
            return WithHttpContext(() => context);
        }

        public FakeHttpRequestBuilder WithHttpContext(
            Func<HttpContext> factory
        )
        {
            return With(
                o => o.SetContext(factory())
            );
        }

        public FakeHttpRequestBuilder WithForm(IFormCollection formCollection)
        {
            return With(
                o => o.Form = formCollection
            );
        }
    }
}