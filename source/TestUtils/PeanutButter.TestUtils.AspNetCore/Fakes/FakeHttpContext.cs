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
using PeanutButter.TestUtils.AspNetCore.Utils;

// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.TestUtils.AspNetCore.Fakes;

/// <summary>
/// Provides a fake http context
/// </summary>
public class FakeHttpContext : HttpContext, IFake
{
    /// <inheritdoc />
    public override void Abort()
    {
        Aborted = true;
    }

    /// <summary>
    /// Indicates if the request has been aborted
    /// </summary>
    public bool Aborted { get; private set; }

    /// <inheritdoc />
    public override IFeatureCollection Features => _features;

    private IFeatureCollection _features;

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public override ConnectionInfo Connection => _connection;

    private ConnectionInfo _connection;

    /// <inheritdoc />
    public override WebSocketManager WebSockets => _webSockets;

    private WebSocketManager _webSockets = WebSocketManagerBuilder.BuildDefault();

    /// <summary>
    /// Sets the websockets manager
    /// </summary>
    /// <param name="webSocketManager"></param>
    public void SetWebSockets(WebSocketManager webSocketManager)
    {
        _webSockets = webSocketManager ?? WebSocketManagerBuilder.BuildDefault();
    }

    /// <inheritdoc />
    [Obsolete(
        "This is obsolete and will be removed in a future version. See https://go.microsoft.com/fwlink/?linkid=845470")]
    public override AuthenticationManager Authentication
        => throw new FeatureIsObsoleteException(nameof(Authentication));

    /// <inheritdoc />
    public override ClaimsPrincipal User { get; set; }

    /// <inheritdoc />
    public override IDictionary<object, object> Items { get; set; } = new Dictionary<object, object>();

    /// <inheritdoc />
    public override IServiceProvider RequestServices { get; set; }
        = new NonFunctionalServiceProvider();

    /// <inheritdoc />
    public override CancellationToken RequestAborted { get; set; } = new();

    /// <inheritdoc />
    public override string TraceIdentifier { get; set; }
        = Guid.NewGuid().ToString();

    /// <inheritdoc />
    public override ISession Session { get; set; }
        = new FakeSession();

    /// <summary>
    /// Sets the features
    /// </summary>
    /// <param name="features"></param>
    public void SetFeatures(IFeatureCollection features)
    {
        _features = features ?? new FakeFeatureCollection();
    }

    /// <summary>
    /// Sets the request accessor
    /// </summary>
    /// <param name="accessor"></param>
    public void SetRequestAccessor(Func<HttpRequest> accessor)
    {
        _requestAccessor = accessor;
    }

    /// <summary>
    /// Sets the response accessor
    /// </summary>
    /// <param name="accessor"></param>
    public void SetResponseAccessor(Func<HttpResponse> accessor)
    {
        _responseAccessor = accessor;
    }

    /// <summary>
    /// Sets the connection info
    /// </summary>
    /// <param name="connection"></param>
    public void SetConnection(ConnectionInfo connection)
    {
        _connection = connection;
    }
}