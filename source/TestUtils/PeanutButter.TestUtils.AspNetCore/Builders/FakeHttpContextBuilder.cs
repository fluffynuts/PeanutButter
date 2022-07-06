using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TestUtils.AspNetCore.Builders;

public class FakeHttpContextBuilder : Builder<FakeHttpContextBuilder, FakeHttpContext>
{
    public static HttpContext BuildDefault()
    {
        return Create().Build();
    }

    public static FakeHttpContextBuilder Create()
    {
        return new();
    }

    internal static FakeHttpContextBuilder CreateWithRequestFactory(
        Func<HttpRequest> requestFactory
    )
    {
        return new(requestFactory);
    }

    public FakeHttpContextBuilder() : this(requestFactory: null)
    {
    }

    internal FakeHttpContextBuilder(Func<HttpRequest> requestFactory)
    {
        WithFeatures(new FakeFeatureCollection())
            .WithResponse(new FakeHttpResponse())
            .WithConnection(GetRandom<FakeConnectionInfo>())
            .WithUser(GetRandom<ClaimsPrincipal>());
        if (requestFactory is null)
        {
            WithRequest(FakeHttpRequestBuilder.BuildDefault());
        }
        else
        {
            WithRequest(requestFactory);
        }
    }

    public FakeHttpContextBuilder WithUser(ClaimsPrincipal principal)
    {
        return With(
            o => o.User = principal
        );
    }

    public FakeHttpContextBuilder WithConnection(
        ConnectionInfo connectionInfo
    )
    {
        return With(
            o => o.SetConnection(connectionInfo)
        );
    }

    public FakeHttpContextBuilder WithResponse(
        FakeHttpResponse response
    )
    {
        return With(o => o.SetResponse(response));
    }

    public FakeHttpContextBuilder WithRequest(
        HttpRequest request
    )
    {
        return WithRequest(() => request);
    }

    public FakeHttpContextBuilder WithRequest(
        Func<HttpRequest> requestFactory
    )
    {
        return With(
            o => o.SetRequest(requestFactory())
        );
    }

    public FakeHttpContextBuilder WithFeatures(
        IFeatureCollection features
    )
    {
        return With(
            o => o.SetFeatures(features ?? new FakeFeatureCollection())
        );
    }

    public FakeHttpContextBuilder WithFormFile(string expected, string name, string fileName = null)
    {
        return With(
            o =>
            {
                var request = o.Request as FakeHttpRequest ??
                    throw new InvalidImplementationException(o.Request, nameof(o.Request));
                var files = request?.Form?.Files as FakeFormFileCollection
                    ?? throw new InvalidImplementationException(request?.Form?.Files, "Request.Form.Files");
                files.Add(new FakeFormFile(expected, name, fileName));
            });
    }
}

public class InvalidImplementationException : Exception
{
    public InvalidImplementationException(
        object implementation,
        string context
    ) : base(GenerateMessageFor(implementation, context))
    {
    }

    private static string GenerateMessageFor(
        object implementation,
        string context
    )
    {
        return implementation is null
            ? $"implementation is null for {context}"
            : $"invalid implementation {implementation} for {context}";
    }
}

public static class FormFileExtensions
{
    public static byte[] ReadAllBytes(
        this IFormFile file
    )
    {
        var target = new MemoryStream();
        using var s = file.OpenReadStream();
        s.CopyTo(target);
        return target.ToArray();
    }

    public static string ReadAllText(
        this IFormFile file
    )
    {
        return Encoding.UTF8.GetString(
            file.ReadAllBytes()
        );
    }
}

public class FakeFormFileCollection : IFormFileCollection
{
    private readonly List<IFormFile> _store = new();

    public IEnumerator<IFormFile> GetEnumerator()
    {
        return _store.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => _store.Count;

    public IFormFile this[int index] => _store[index];

    public IFormFile GetFile(string name)
    {
        return _store.FirstOrDefault(o => o.Name == name);
    }

    public IReadOnlyList<IFormFile> GetFiles(string name)
    {
        return _store
            .Where(o => o.Name == name)
            .ToList();
    }

    public IFormFile this[string name] => GetFile(name);

    public void Add(IFormFile formFile)
    {
        _store.Add(formFile);
    }
}

public class FakeHttpResponse : HttpResponse
{
    public override void OnStarting(Func<object, Task> callback, object state)
    {
    }

    public override void OnCompleted(Func<object, Task> callback, object state)
    {
    }

    public override void Redirect(string location, bool permanent)
    {
    }

    public override HttpContext HttpContext { get; }
    public override int StatusCode { get; set; }
    public override IHeaderDictionary Headers { get; }
    public override Stream Body { get; set; }
    public override long? ContentLength { get; set; }
    public override string ContentType { get; set; }
    public override IResponseCookies Cookies { get; }
    public override bool HasStarted { get; }
}