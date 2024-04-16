using System.IO;
using Microsoft.AspNetCore.Http;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore.Utils;
#else
namespace PeanutButter.TestUtils.AspNetCore.Utils;
#endif

/// <summary>
/// Null-pattern: simply produces a new stream
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class NullBodyEncoder : IFormEncoder
{
    /// <inheritdoc />
    public Stream Encode(IFormCollection form)
    {
        return new MemoryStream();
    }
}