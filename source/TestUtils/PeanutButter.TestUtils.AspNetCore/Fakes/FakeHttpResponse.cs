using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.TestUtils.AspNetCore.Fakes;

/// <summary>
/// Provides a fake http response
/// </summary>
public class FakeHttpResponse : HttpResponse, IFake
{
    /// <summary>
    /// The default content type for a response
    /// </summary>
    public const string DEFAULT_CONTENT_TYPE = "text/html";

    /// <inheritdoc />
    public override void OnStarting(Func<object, Task> callback, object state)
    {
        _onStarting.ForEach(func => func?.Invoke(callback, state));
    }

    /// <inheritdoc />
    public override void OnCompleted(Func<object, Task> callback, object state)
    {
        _onCompleted.ForEach(func => func?.Invoke(callback, state));
    }

    /// <inheritdoc />
    public override void Redirect(string location, bool permanent)
    {
        StatusCode = permanent
            ? (int)HttpStatusCode.PermanentRedirect
            : (int)HttpStatusCode.Redirect;
        Headers.Location = location;
        _onRedirect.ForEach(func => func?.Invoke(location, permanent));
    }

    /// <inheritdoc />
    public override HttpContext HttpContext =>
        _httpContext ??= _httpContextAccessor?.Invoke();

    private Func<HttpContext> _httpContextAccessor;
    private HttpContext _httpContext;

    /// <inheritdoc />
    public override int StatusCode { get; set; }

    /// <inheritdoc />
    public override IHeaderDictionary Headers =>
        _headers;

    private IHeaderDictionary _headers = new FakeHeaderDictionary();

    /// <inheritdoc />
    public override Stream Body
    {
        get => _body;
        set
        {
            if (object.ReferenceEquals(_body, value))
            {
                return;
            }

            _body = value;
            _pipeWriter = new FakeResponsePipeWriter(this);
        }
    }

    private Stream _body = new MemoryStream();

    /// <inheritdoc />
    public override long? ContentLength
    {
        get => Body.Length;
        set => Body.SetLength(value ?? 0);
    }

    /// <inheritdoc />
    public override string ContentType { get; set; } = DEFAULT_CONTENT_TYPE;

    /// <inheritdoc />
    public override IResponseCookies Cookies => _cookies;

    private IResponseCookies _cookies;

    /// <inheritdoc />
    public override bool HasStarted => _hasStarted;

    private bool _hasStarted;

    /// <inheritdoc />
    public override PipeWriter BodyWriter =>
        _pipeWriter ??= new FakeResponsePipeWriter(this);

    private PipeWriter _pipeWriter;

    /// <summary>
    /// Set the http context accessor
    /// </summary>
    /// <param name="accessor"></param>
    public void SetContextAccessor(
        Func<HttpContext> accessor
    )
    {
        _httpContext = null;
        _httpContextAccessor = accessor;
    }

    /// <summary>
    /// Set the response headers collection
    /// </summary>
    /// <param name="headers"></param>
    public void SetHeaders(IHeaderDictionary headers)
    {
        _headers = Headers;
    }

    /// <summary>
    /// Set the cookies collection
    /// </summary>
    /// <param name="cookies"></param>
    public void SetCookies(IResponseCookies cookies)
    {
        _cookies = cookies;
        if (cookies is not FakeResponseCookies fake)
        {
            return;
        }

        if (ReferenceEquals(fake.HttpResponse, this))
        {
            return;
        }

        fake.HttpResponse = this;
    }

    private readonly List<Action<Func<object, Task>, object>> _onStarting = new();

    /// <summary>
    /// Add a handler for when the request is starting
    /// </summary>
    /// <param name="handler"></param>
    public void AddOnStartingHandler(
        Action<Func<object, Task>, object> handler
    )
    {
        _onStarting.Add(handler);
    }

    private readonly List<Action<Func<object, Task>, object>> _onCompleted = new();

    /// <summary>
    /// Add a handler for when the request is completed
    /// </summary>
    /// <param name="handler"></param>
    public void AddOnCompletedHandler(
        Action<Func<object, Task>, object> handler
    )
    {
        _onCompleted.Add(handler);
    }

    private readonly List<Action<string, bool>> _onRedirect = new();

    /// <summary>
    /// Add a handler for when the request is redirected
    /// </summary>
    /// <param name="handler"></param>
    public void AddRedirectHandler(
        Action<string, bool> handler
    )
    {
        _onRedirect.Add(handler);
    }

    /// <summary>
    /// Set the HasStarted property
    /// </summary>
    /// <param name="hasStarted"></param>
    public void SetHasStarted(bool hasStarted)
    {
        _hasStarted = hasStarted;
    }

    /// <summary>
    /// Does nothing
    /// </summary>
    /// <returns></returns>
    public override Task CompleteAsync()
    {
        // TODO: invoke OnCompleted handlers?
        SetHasStarted(true);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing, really
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task StartAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        SetHasStarted(true);
        return Task.CompletedTask;
    }
}