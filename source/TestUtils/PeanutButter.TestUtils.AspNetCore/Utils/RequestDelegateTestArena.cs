using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PeanutButter.TestUtils.AspNetCore.Builders;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TestUtils.AspNetCore.Utils;

/// <summary>
/// Sets up the useful bits for testing an asp.net middleware
/// class - use this to easily set up the context and next
/// parameters for InvokeAsync
/// </summary>
public class RequestDelegateTestArena
{
    /// <summary>
    /// The delegate that is generated for your test
    /// </summary>
    public RequestDelegate Delegate { get; }

    /// <summary>
    /// The HttpContext (or use this to set your own)
    /// </summary>
    public HttpContext HttpContext
    {
        get => _httpContext ??= CreateRandomHttpContext();
        set => _httpContext = value;
    }

    private readonly Action<HttpContextBuilder> _httpContextMutator;

    private HttpContext CreateRandomHttpContext()
    {
        var builder = HttpContextBuilder.Create()
            .WithRequest(
                HttpRequestBuilder.Create()
                    .WithUrl(GetRandomHttpsUrl())
                    .WithMethod(
                        GetRandomFrom(
                            new[] { "GET", "POST", "PUT", "DELETE" }
                        )
                    )
                    .Build()
            );
        _httpContextMutator(builder);
        return builder.Build();
    }

    private HttpContext _httpContext;

    private const string METADATA_KEY_CALL_ARGS = "__callargs__";

    /// <summary>
    /// Construct a test arena with no custom logic in the delegate
    /// and no mutations to the http context
    /// </summary>
    public RequestDelegateTestArena() : this(
        GenerateDefaultNextLogic(),
        NoOp
    )
    {
    }

    /// <summary>
    /// Create the test arena with custom delegate logic and
    /// no http context mutations
    /// </summary>
    /// <param name="delegateLogic"></param>
    public RequestDelegateTestArena(
        Action<HttpContext> delegateLogic
    ) : this(WrapSynchronousLogic(delegateLogic))
    {
    }

    internal static Func<HttpContext, Task> WrapSynchronousLogic(
        Action<HttpContext> delegateLogic
    )
    {
        return ctx =>
        {
            delegateLogic.Invoke(ctx);
            return Task.CompletedTask;
        };
    }

    /// <summary>
    /// Create the test arena with custom delegate logic and
    /// no http context mutations
    /// </summary>
    /// <param name="delegateLogic"></param>
    public RequestDelegateTestArena(
        Func<HttpContext, Task> delegateLogic
    ) : this(delegateLogic, NoOp)
    {
    }

    /// <summary>
    /// Create the test arena with http context mutations
    /// and no custom delegate logic
    /// </summary>
    /// <param name="httpContextMutator"></param>
    public RequestDelegateTestArena(
        Action<HttpContextBuilder> httpContextMutator
    ) : this(GenerateDefaultNextLogic(), httpContextMutator)
    {
    }

    private static Func<HttpContext, Task> GenerateDefaultNextLogic()
    {
        return _ => Task.CompletedTask;
    }

    /// <summary>
    /// Create the test arena with the provided delegate
    /// logic and http context mutations
    /// </summary>
    /// <param name="logic"></param>
    /// <param name="httpContextMutator"></param>
    public RequestDelegateTestArena(
        Func<HttpContext, Task> logic,
        Action<HttpContextBuilder> httpContextMutator
    )
    {
        var logic1 = logic ?? throw new ArgumentNullException(nameof(logic));
        _httpContextMutator = httpContextMutator ?? throw new ArgumentNullException(nameof(httpContextMutator));
        Delegate = requestContext =>
        {
            lock (Delegate!)
            {
                if (!Delegate.TryGetMetadata<List<HttpContext>>(METADATA_KEY_CALL_ARGS, out var argsList))
                {
                    argsList = new List<HttpContext>();
                    Delegate.SetMetadata(METADATA_KEY_CALL_ARGS, argsList);
                }

                argsList.Add(requestContext);
            }

            return logic1.Invoke(requestContext);
        };

        Delegate.SetMetadata(METADATA_KEY_CALL_ARGS, new List<HttpContext>());
    }

    /// <summary>
    /// Construct with the provided logic and context
    /// </summary>
    /// <param name="logic"></param>
    /// <param name="httpContext"></param>
    /// <exception cref="NotImplementedException"></exception>
    public RequestDelegateTestArena(
        Func<HttpContext, Task> logic,
        HttpContext httpContext
    ): this(logic, NoOp)
    {
        _httpContext = httpContext;
    }

    private static void NoOp(HttpContextBuilder _)
    {
    }

    /// <summary>
    /// Deconstruct into a tuple, eg
    /// var (ctx, next) = new RequestDelegateTestArena();
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="next"></param>
    public void Deconstruct(
        out HttpContext ctx,
        out RequestDelegate next
    )
    {
        ctx = HttpContext;
        next = Delegate;
    }
}