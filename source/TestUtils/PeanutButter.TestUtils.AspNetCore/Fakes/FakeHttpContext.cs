using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features;

namespace PeanutButter.TestUtils.AspNetCore.Fakes
{
    public class FakeHttpContext : HttpContext
    {
        public override void Abort()
        {
        }

        public override IFeatureCollection Features => _features;
        private IFeatureCollection _features;

        public override HttpRequest Request => 
            _request ??= _requestAccessor();
        private HttpRequest _request;
        private Func<HttpRequest> _requestAccessor;
        public override HttpResponse Response => _response;
        private HttpResponse _response;

        public override ConnectionInfo Connection => _connection;
        private ConnectionInfo _connection;

        public override WebSocketManager WebSockets { get; }

        [Obsolete(
            "This is obsolete and will be removed in a future version. See https://go.microsoft.com/fwlink/?linkid=845470")]
        public override AuthenticationManager Authentication
            => throw new FeatureIsObsoleteException(nameof(Authentication));

        public override ClaimsPrincipal User { get; set; }
        public override IDictionary<object, object> Items { get; set; }
        public override IServiceProvider RequestServices { get; set; }
        public override CancellationToken RequestAborted { get; set; }
        public override string TraceIdentifier { get; set; }
        public override ISession Session { get; set; }

        public void SetFeatures(IFeatureCollection features)
        {
            _features = features;
        }

        public void SetRequest(HttpRequest request)
        {
            SetRequestAccessor(() => request);
        }

        public void SetRequestAccessor(Func<HttpRequest> accessor)
        {
            _requestAccessor = accessor;
        }

        public void SetResponse(FakeHttpResponse response)
        {
            _response = response;
        }

        public void SetConnection(ConnectionInfo connection)
        {
            _connection = connection;
        }
    }
}