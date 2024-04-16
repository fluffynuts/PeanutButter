using System.IO;
using Microsoft.AspNetCore.Http;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore.Utils;
#else
namespace PeanutButter.TestUtils.AspNetCore.Utils;
#endif

/// <summary>
/// Decodes a form
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    interface IFormDecoder
{
    /// <summary>
    /// Decodes a form
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    public IFormCollection Decode(Stream body);
}