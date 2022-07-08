using System;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Http;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.AspNetCore.Utils;

/// <summary>
/// Encodes a multi-part form
/// </summary>
public class MultiPartBodyEncoder : IFormEncoder
{
    /// <summary>
    /// The boundary marker
    /// </summary>
    public const string BOUNDARY = "--boundary";
    /// <summary>
    /// The Content-Disposition marker
    /// </summary>
    public const string CONTENT_DISPOSITION = "Content-Disposition";
    /// <summary>
    /// The Content-Type marker
    /// </summary>
    public const string CONTENT_TYPE = "Content-Type";
    /// <summary>
    /// The Content-Length marker
    /// </summary>
    public const string CONTENT_LENGTH = "Content-Length";
    /// <summary>
    /// The name key
    /// </summary>
    public const string NAME = "name";
    /// <summary>
    /// The filename key
    /// </summary>
    public const string FILE_NAME = "filename";

    /// <inheritdoc />
    public Stream Encode(IFormCollection form)
    {
        var result = new MemoryStream();
        if (form is null)
        {
            return result;
        }

        foreach (var key in form.Keys)
        {
            result.AppendLine(BOUNDARY);
            result.AppendLine($"{CONTENT_DISPOSITION}: form-data; {NAME}=\"{WebUtility.UrlEncode(key)}\"");
            result.AppendLine("");
            result.AppendLine(form[key]);
        }

        foreach (var file in form.Files)
        {
            result.AppendLine(BOUNDARY);
            result.AppendLine(
                $"{CONTENT_DISPOSITION}: {NAME}=\"{WebUtility.UrlEncode(file.Name)}\"; {FILE_NAME}=\"{WebUtility.UrlEncode(file.FileName)}\""
            );
            result.AppendLine($"{CONTENT_TYPE}: {file.ContentType}");
            result.AppendLine($"{CONTENT_LENGTH}: {file.Length}");
            result.AppendLine("");
            if (file.IsText())
            {
                using var s = file.OpenReadStream();
                s.CopyTo(result);
                result.AppendLine("");
            }
            else
            {
                using var s = file.OpenReadStream();
                var base64 = Convert.ToBase64String(s.ReadAllBytes());
                result.AppendLine(base64);
            }
        }

        result.Position = 0;
        return result;
    }
}