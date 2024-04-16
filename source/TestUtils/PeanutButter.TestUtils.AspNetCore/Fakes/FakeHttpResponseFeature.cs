using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore.Fakes;
#else
namespace PeanutButter.TestUtils.AspNetCore.Fakes;
#endif

/// <summary>
/// Provides a faked IHttpResponseFeature
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class FakeHttpResponseFeature : IHttpResponseFeature
{
    /// <summary>
    /// Constructs a new faked HttpResponseFeature, bound to the
    /// HttpResponse provided by the factory
    /// </summary>
    /// <param name="factory"></param>
    public FakeHttpResponseFeature(
        Func<HttpResponse> factory
    )
    {
        _factory = factory;
        ReasonPhrase = ParseStatusCode(StatusCode);
    }

    private int? _lastStatusCode;
    private string _reasonPhrase;
    private readonly Func<HttpResponse> _factory;

    private string ParseStatusCode(int statusCode)
    {
        _lastStatusCode = statusCode;
        return Enum.TryParse($"{statusCode}", out HttpStatusCode parsed)
            ? $"{parsed}"
            : "";
    }

    /// <inheritdoc />
    public void OnStarting(Func<object, Task> callback, object state)
    {
    }

    /// <inheritdoc />
    public void OnCompleted(Func<object, Task> callback, object state)
    {
    }

    /// <inheritdoc />
    public int StatusCode
    {
        get => Response.StatusCode;
        set => Response.StatusCode = value;
    }

    /// <inheritdoc />
    public string ReasonPhrase
    {
        get => UpdateReasonPhraseIfNecessary();
        set => _reasonPhrase = value;
    }

    private string UpdateReasonPhraseIfNecessary()
    {
        if (Response.StatusCode == _lastStatusCode)
        {
            return _reasonPhrase;
        }

        return _reasonPhrase = ParseStatusCode(_factory().StatusCode);
    }

    /// <inheritdoc />
    public IHeaderDictionary Headers
    {
        get => Response.Headers;
        set
        {
            if (Response is not FakeHttpResponse fake)
            {
                throw new NotSupportedException(
                    $"Headers may only be completely replaced if the underlying response is a {nameof(FakeHttpResponse)}"
                );
            }

            fake.SetHeaders(value);
        }
    }

    /// <inheritdoc />
    public Stream Body
    {
        get => Response.Body;
        set => Response.Body = value;
    }

    /// <inheritdoc />
    public bool HasStarted => Response.HasStarted;

    private HttpResponse Response => _factory() ?? throw new InvalidOperationException(
        "No response available"
    );
}