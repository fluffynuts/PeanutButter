using System.IO;
using Microsoft.AspNetCore.Http;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore.Utils;
#else
namespace PeanutButter.TestUtils.AspNetCore.Utils;
#endif

/// <summary>
/// Encodes a form
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    interface IFormEncoder
{
    /// <summary>
    /// Encodes a form
    /// </summary>
    /// <param name="form"></param>
    /// <returns></returns>
    public Stream Encode(IFormCollection form);
}