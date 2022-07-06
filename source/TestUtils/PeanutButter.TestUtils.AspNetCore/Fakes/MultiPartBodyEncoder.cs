using System;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Http;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.AspNetCore.Fakes;

public class MultiPartBodyEncoder : IFormEncoder
{
    public const string BOUNDARY = "--boundary";
    public const string CONTENT_DISPOSITION = "Content-Disposition";
    public const string CONTENT_TYPE = "Content-Type";
    public const string CONTENT_LENGTH = "Content-Length";
    public const string NAME = "name";
    public const string FILE_NAME = "filename";

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