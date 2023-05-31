using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace PeanutButter.TestUtils.AspNetCore.Builders;

/// <summary>
/// simplify some routines around HttpRequests
/// </summary>
public static class HttpRequestExtensions
{
    /// <summary>
    /// Set the request up as if it came from the provided url
    /// </summary>
    /// <param name="request"></param>
    /// <param name="url"></param>
    public static void SetUrl(
        this HttpRequest request,
        Uri url
    )
    {
        request.Scheme = url.Scheme;
        var hasDefaultPort = DefaultPorts.TryGetValue(url.Scheme, out var port)
            && port == url.Port;
        request.Host = hasDefaultPort
            ? new HostString(url.Host)
            : new HostString(url.Host, url.Port);
        request.Path = url.AbsolutePath;
        request.PathBase = "";
        request.QueryString = new QueryString(url.Query);
    }

    /// <summary>
    /// Set the request up as if it came from the provided url
    /// </summary>
    /// <param name="request"></param>
    /// <param name="url"></param>
    public static void SetUrl(
        this HttpRequest request,
        string url
    )
    {
        request.SetUrl(
            new Uri(url)
        );
    }

    private static readonly Dictionary<string, int> DefaultPorts = new(StringComparer.OrdinalIgnoreCase)
    {
        ["http"] = 80,
        ["https"] = 443
    };
}