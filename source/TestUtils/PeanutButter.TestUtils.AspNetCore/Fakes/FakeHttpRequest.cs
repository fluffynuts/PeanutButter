using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.AspNetCore.Fakes
{
    public class FakeHttpRequest : HttpRequest
    {
        public override Task<IFormCollection> ReadFormAsync(
            CancellationToken cancellationToken = new CancellationToken()
        )
        {
            return Task.FromResult<IFormCollection>(
                new FakeFormCollection()
            );
        }

        public override HttpContext HttpContext => _httpContext;
        private HttpContext _httpContext;

        public override string Method { get; set; }
        public override string Scheme { get; set; }

        public override bool IsHttps
        {
            get => Scheme?.ToLower() == "https";
            set => Scheme = value
                ? "https"
                : "http";
        }

        public override HostString Host { get; set; }
        public override PathString PathBase { get; set; }
        public override PathString Path { get; set; }

        public override QueryString QueryString
        {
            get => _queryString;
            set
            {
                _queryString = value;
                _query = new FakeQueryCollection(_queryString.ToString());
            }
        }

        private QueryString _queryString;

        public override IQueryCollection Query
        {
            get => _query ??= new FakeQueryCollection();
            set
            {
                _query = value ?? new FakeQueryCollection();
                _queryString = GenerateQueryStringFrom(_query);
            }
        }

        private IQueryCollection _query;
        private IHeaderDictionary _headers;

        public override string Protocol
        {
            get => Scheme;
            set => Scheme = value;
        }

        public override IHeaderDictionary Headers
            => _headers ??= new FakeHeaderDictionary();

        public override IRequestCookieCollection Cookies { get; set; } = new FakeRequestCookieCollection();
        public override long? ContentLength { get; set; } = 0;
        public override string ContentType { get; set; }

        public override Stream Body
        {
            get => _body ??= new MemoryStream();
            set => _body = value ?? new MemoryStream();
        }

        private Stream _body;

        public override bool HasFormContentType =>
            (Form?.Keys.Count ?? 0) > 0;

        public override IFormCollection Form
        {
            get => _form;
            set
            {
                _form = value ?? new FakeFormCollection();
                UpdateBodyForForm();
            }
        }

        private void UpdateBodyForForm()
        {
            ContentType = SelectContentTypeForFormOrBody();
            if (ContentType == MULTIPART_FORM)
            {
                GenerateMultiPartBody();
            }
            else if (ContentType == URLENCODED_FORM)
            {
                _body = GenerateUrlEncodedBody();
            }
        }

        private MemoryStream GenerateUrlEncodedBody()
        {
            var result = new MemoryStream();
            var isFirst = true;
            foreach (var key in Form.Keys)
            {
                if (!isFirst)
                {
                    result.AppendString("&");
                }

                isFirst = false;

                result.AppendString($"{key}={WebUtility.UrlEncode(Form[key])}");
            }

            result.Position = 0;
            return result;
        }

        private MemoryStream GenerateMultiPartBody()
        {
            var result = new MemoryStream();

            foreach (var key in Form.Keys)
            {
                result.AppendLine("--boundary");
                result.AppendLine($"Content-Disposition: form-data; name=\"{key}\"");
                result.AppendLine("");
                result.AppendLine(Form[key]);
            }

            foreach (var file in Form.Files)
            {
                result.AppendLine("--boundary");
                result.AppendLine(
                    $"Content-Disposition: form-data; name=\"{file.Name}\"; filename=\"{file.FileName}\"");
                result.AppendLine($"Content-Type: {file.ContentType}");
                result.AppendLine($"Content-Length: {file.Length}");
                result.AppendLine("");
                if (file.IsText())
                {
                    using var s = file.OpenReadStream();
                    s.CopyTo(result);
                }
                else
                {
                    using var s = file.OpenReadStream();
                    var base64 = Convert.ToBase64String(s.ReadAllBytes());
                    result.AppendLine(base64);
                }
            }

            return result;
        }

        private const string MULTIPART_FORM = "multipart/form-data";
        private const string URLENCODED_FORM = "application/x-www-form-urlencoded";

        private string SelectContentTypeForFormOrBody()
        {
            if (_form.Files.Any())
            {
                return MULTIPART_FORM;
            }

            if (_form.Keys.Any())
            {
                return "application/x-www-form-urlencoded";
            }

            if ((_body?.Length ?? 0) == 0)
            {
                return "text/plain";
            }

            // assume json, for now
            return "application/json";
        }

        private IFormCollection _form;

        public void SetContext(HttpContext context)
        {
            _httpContext = context;
        }

        private QueryString GenerateQueryStringFrom(IQueryCollection query)
        {
            var parts = new List<string>();
            foreach (var key in query.Keys)
            {
                parts.Add($"{key}={WebUtility.UrlEncode(query[key])}");
            }

            return parts.Any()
                ? new QueryString($"?{string.Join("&", parts)}")
                : new QueryString();
        }

        public void SetHeaders(IHeaderDictionary headers)
        {
            _headers = headers;
        }
    }
}

public static class StreamExtensions
{
    public static void AppendString(
        this Stream stream,
        string value
    )
    {
        stream.Append(
            Encoding.UTF8.GetBytes(value)
        );
    }

    public static void AppendLine(
        this Stream stream,
        string line
    )
    {
        stream.AppendString($"{line}\n");
    }
}

public static class FormFileExtensions
{
    public static bool IsText(
        this IFormFile file
    )
    {
        var contentType = file.ContentType ?? "";
        return contentType.StartsWith("text/") || 
            TextMimeTypes.Contains(file.ContentType);
    }

    private static readonly HashSet<string> TextMimeTypes = new(
        new[]
        {
            "image/svg+xml",
            "image/svg",
            "application/json"
        }
    );
}