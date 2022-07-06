using Microsoft.AspNetCore.Http;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.AspNetCore.Fakes;

public static class FormFileExtensions
{
    public static bool IsText(
        this IFormFile file
    )
    {
        var contentType = file.ContentType ?? "";
        return contentType.IsTextMIMEType();
    }
}