using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PeanutButter.RandomGenerators;

namespace PeanutButter.TestUtils.AspNetCore.Fakes
{
    public class FakeHttpResponse : HttpResponse
    {
        public const string DEFAULT_CONTENT_TYPE = "text/html";

        public override void OnStarting(Func<object, Task> callback, object state)
        {
            _onStarting.ForEach(func => func?.Invoke(callback, state));
        }

        public override void OnCompleted(Func<object, Task> callback, object state)
        {
            _onCompleted.ForEach(func => func?.Invoke(callback, state));
        }

        public override void Redirect(string location, bool permanent)
        {
            _onRedirect.ForEach(func => func?.Invoke(location, permanent));
        }

        public override HttpContext HttpContext =>
            _httpContext ??= _httpContextAccessor?.Invoke();

        private Func<HttpContext> _httpContextAccessor;
        private HttpContext _httpContext;

        public override int StatusCode { get; set; }

        public override IHeaderDictionary Headers =>
            _headers;

        private IHeaderDictionary _headers = new FakeHeaderDictionary();

        public override Stream Body { get; set; } = new MemoryStream();

        public override long? ContentLength
        {
            get => Body.Length;
            set => Body.SetLength(value ?? 0);
        }

        public override string ContentType { get; set; } = DEFAULT_CONTENT_TYPE;
        public override IResponseCookies Cookies => _cookies;
        private IResponseCookies _cookies = FakeResponseCookies.Create();

        public override bool HasStarted => _hasStarted;
        private bool _hasStarted;

        public void SetContextAccessor(
            Func<HttpContext> accessor
        )
        {
            _httpContextAccessor = accessor;
        }

        public void SetHeaders(IHeaderDictionary headers)
        {
            _headers = Headers;
        }

        public void SetCookies(IResponseCookies cookies)
        {
            _cookies = cookies;
        }

        private readonly List<Action<Func<object, Task>, object>> _onStarting = new();

        public void AddOnStartingHandler(
            Action<Func<object, Task>, object> handler
        )
        {
            _onStarting.Add(handler);
        }

        private readonly List<Action<Func<object, Task>, object>> _onCompleted = new();

        public void AddOnCompletedHandler(
            Action<Func<object, Task>, object> handler
        )
        {
            _onCompleted.Add(handler);
        }

        private readonly List<Action<string, bool>> _onRedirect = new();

        public void AddRedirectHandler(
            Action<string, bool> handler
        )
        {
            _onRedirect.Add(handler);
        }

        public void SetHasStarted(bool hasStarted)
        {
            _hasStarted = hasStarted;
        }
    }

    public class Cookie
    {
        public string Key { get; }
        public string Value { get; }
        public CookieOptions Options { get; }

        public Cookie(
            string key,
            string value,
            CookieOptions options
        )
        {
            Key = key;
            Value = value;
            Options = options;
        }
    }

    public class FakeResponseCookies : IResponseCookies
    {
        public IDictionary<string, Cookie> Store
            => _store;

        private readonly Dictionary<string, Cookie> _store = new();

        public static IResponseCookies Create()
        {
            // try to return a substitute if possible
            // -> then assertions against setting cookies is easier
            return GenericBuilderBase.TryCreateSubstituteFor<FakeResponseCookies>(
                callThrough: true,
                out var result
            )
                ? result
                : new FakeResponseCookies();
        }

        public virtual void Append(string key, string value)
        {
            Store[key] = new Cookie(key, value, new CookieOptions());
        }

        public virtual void Append(string key, string value, CookieOptions options)
        {
            throw new NotImplementedException();
        }

        public virtual void Delete(string key)
        {
            throw new NotImplementedException();
        }

        public virtual void Delete(string key, CookieOptions options)
        {
            throw new NotImplementedException();
        }
    }
}