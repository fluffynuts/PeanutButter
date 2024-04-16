using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using PeanutButter.Utils;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore.Utils;
#else
namespace PeanutButter.TestUtils.AspNetCore.Utils;
#endif

/// <summary>
/// Provides some convenience extensions for form files
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    static class FormFileExtensions
{
    /// <summary>
    /// Read all bytes of the file
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static byte[] ReadAllBytes(
        this IFormFile file
    )
    {
        var target = new MemoryStream();
        using var s = file.OpenReadStream();
        s.CopyTo(target);
        return target.ToArray();
    }

    /// <summary>
    /// Read all text of the file
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static string ReadAllText(
        this IFormFile file
    )
    {
        return Encoding.UTF8.GetString(
            file.ReadAllBytes()
        );
    }

    /// <summary>
    /// Returns true if the form file appears to be text by the associated mime type
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static bool IsText(
        this IFormFile file
    )
    {
        var contentType = file.ContentType ?? "";
        return contentType.IsTextMIMEType();
    }
}