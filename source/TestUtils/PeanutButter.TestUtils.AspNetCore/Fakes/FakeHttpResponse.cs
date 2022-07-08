using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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
        private IResponseCookies _cookies = FakeResponseCookies.CreateSubstitutedIfPossible();

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
}