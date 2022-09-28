using System;
using Microsoft.AspNetCore.Http;

namespace PeanutButter.TestUtils.AspNetCore;

// TODO: if there's ever more asp.net utils,
// move this into that project to publish

/// <summary>
/// Provides convenience HttpRequestExtensions
/// </summary>
public static class HttpRequestExtensions
{
    /// <summary>
    /// Groks the full url for an HttpRequest into an Uri
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static Uri FullUrl(
        this HttpRequest request
    )
    {
        return new Uri($@"{
            request.Scheme
        }://{
            request.Host
        }{
            request.Path
        }{request.QueryString}"
        );
    }
}