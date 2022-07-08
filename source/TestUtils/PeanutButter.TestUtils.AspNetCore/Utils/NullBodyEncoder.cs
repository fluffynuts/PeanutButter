using System.IO;
using Microsoft.AspNetCore.Http;

namespace PeanutButter.TestUtils.AspNetCore.Utils;

/// <summary>
/// Null-pattern: simply produces a new stream
/// </summary>
public class NullBodyEncoder : IFormEncoder
{
    /// <inheritdoc />
    public Stream Encode(IFormCollection form)
    {
        return new MemoryStream();
    }
}