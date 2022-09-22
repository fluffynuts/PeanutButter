using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.AspNetCore.Fakes;

/// <summary>
/// Provides a fake cookie collection
/// </summary>
public class FakeRequestCookieCollection
    : StringMap, IRequestCookieCollection, IFake
{
    /// <summary>
    /// The associated HttpRequest
    /// </summary>
    public HttpRequest HttpRequest
    {
        get => _httpRequest;
        set
        {
            var firstSet = _httpRequest is null;
            _httpRequest = value;
            if (firstSet)
            {
                SetRequestCookieHeaderFromStore();
            }
        }
    }

    private HttpRequest _httpRequest;
    private int? _headerHashCode;

    /// <summary>
    /// Before the value is retrieved, ensure we're in sync with any associated
    /// HttpContext's request
    /// </summary>
    /// <param name="key"></param>
    protected override string Retrieve(string key)
    {
        UpdateCookiesFromHeadersIfHeadersHaveChanged();
        return base.Retrieve(key);
    }

    private void UpdateCookiesFromHeadersIfHeadersHaveChanged()
    {
        if (RequestCookieHeaderHasChanged())
        {
            SetCookiesFromRequestCookieHeader();
        }
    }

    private bool RequestCookieHeaderHasChanged()
    {
        var hashCode = HttpRequest?.Headers[CookieUtil.HEADER].GetHashCode();
        var result = hashCode != _headerHashCode;
        _headerHashCode = _headerHashCode = hashCode;
        return result;
    }

    /// <inheritdoc />
    public override IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        UpdateCookiesFromHeadersIfHeadersHaveChanged();
        return base.GetEnumerator();
    }

    private void SetCookiesFromRequestCookieHeader()
    {
        Clear();
        if (HttpRequest is null)
        {
            return;
        }

        var header = HttpRequest.Headers[CookieUtil.HEADER].FirstOrDefault() ?? "";
        var parts = header.Split(new[] { CookieUtil.DELIMITER }, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .ToArray();
        foreach (var part in parts)
        {
            var subs = part.SplitOnce("=");
            var key = subs[0];
            var value = subs.Length > 1
                ? subs[1]
                : "";
            base.Store(key, value);
        }

        _headerHashCode = HttpRequest.Headers.GetHashCode();
    }

    /// <summary>
    /// After storing, ensure that any associated http context's request headers
    /// reflect the same truth
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    protected override void Store(string key, string value)
    {
        base.Store(key, value);
        SetRequestCookieHeaderFromStore();
    }

    private void SetRequestCookieHeaderFromStore()
    {
        if (HttpRequest is null)
        {
            return;
        }

        CookieUtil.GenerateCookieHeader(this, HttpRequest, overwrite: true);
        _headerHashCode = HttpRequest.Headers[CookieUtil.HEADER].GetHashCode();
        
    }
}