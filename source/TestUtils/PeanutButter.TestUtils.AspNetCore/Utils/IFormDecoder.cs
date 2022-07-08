using System.IO;
using Microsoft.AspNetCore.Http;

namespace PeanutButter.TestUtils.AspNetCore.Utils;

/// <summary>
/// Decodes a form
/// </summary>
public interface IFormDecoder
{
    /// <summary>
    /// Decodes a form
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    public IFormCollection Decode(Stream body);
}