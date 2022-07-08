using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features;
using PeanutButter.TestUtils.AspNetCore.Builders;

// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.TestUtils.AspNetCore.Fakes
{
    public class FakeHttpContext : HttpContext
    {
        public override void Abort()
        {
            Aborted = true;
        }

        public bool Aborted { get; private set; }

        public override IFeatureCollection Features => _features;
        private IFeatureCollection _features;

        public override HttpRequest Request =>
            _request ??= TrySetContextOn(_requestAccessor?.Invoke());

        private HttpRequest TrySetContextOn(HttpRequest req)
        {
            if (req is FakeHttpRequest fake)
            {
                fake.SetContextAccessor(() => this);
            }
            else
            {
                TrySetSingleContextField(req, this);
            }

            return req;
        }

        private static void TrySetSingleContextField(
            object o,
            FakeHttpContext context
        )
        {
            if (o is null)
            {
                return;
            }

            var fields = o.GetType()
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(f => f.FieldType.IsAssignableFrom(typeof(HttpContext)))
                .ToArray();
            if (fields.Length == 1)
            {
                fields[0].SetValue(o, context);
            }
        }

        private HttpRequest _request;
        private Func<HttpRequest> _requestAccessor;

        public override HttpResponse Response =>
            _response ??= TrySetContextOn(_responseAccessor?.Invoke());

        private HttpResponse TrySetContextOn(HttpResponse res)
        {
            if (res is FakeHttpResponse fake)
            {
                fake.SetContextAccessor(() => this);
            }
            else
            {
                TrySetSingleContextField(res, this);
            }

            return res;
        }

        private HttpResponse _response;
        private Func<HttpResponse> _responseAccessor;

        public override ConnectionInfo Connection => _connection;
        private ConnectionInfo _connection;

        public override WebSocketManager WebSockets => _webSockets;
        private WebSocketManager _webSockets = WebSocketManagerBuilder.BuildDefault();

        public void SetWebSockets(WebSocketManager webSocketManager)
        {
            _webSockets = webSocketManager ?? WebSocketManagerBuilder.BuildDefault();
        }

        [Obsolete(
            "This is obsolete and will be removed in a future version. See https://go.microsoft.com/fwlink/?linkid=845470")]
        public override AuthenticationManager Authentication
            => throw new FeatureIsObsoleteException(nameof(Authentication));

        public override ClaimsPrincipal User { get; set; }
        public override IDictionary<object, object> Items { get; set; } = new Dictionary<object, object>();

        public override IServiceProvider RequestServices { get; set; }
            = new NonFunctionalServiceProvider();

        public override CancellationToken RequestAborted { get; set; } = new();

        public override string TraceIdentifier { get; set; }
            = Guid.NewGuid().ToString();

        public override ISession Session { get; set; }
            = new FakeSession();

        public void SetFeatures(IFeatureCollection features)
        {
            _features = features ?? new FakeFeatureCollection();
        }

        public void SetRequest(HttpRequest request)
        {
            SetRequestAccessor(() => request);
        }

        public void SetRequestAccessor(Func<HttpRequest> accessor)
        {
            _requestAccessor = accessor;
        }

        public void SetResponseAccessor(Func<HttpResponse> accessor)
        {
            _responseAccessor = accessor;
        }

        public void SetConnection(ConnectionInfo connection)
        {
            _connection = connection;
        }
    }
}