using System.IO;
using Microsoft.AspNetCore.Http;

namespace PeanutButter.TestUtils.AspNetCore.Utils;

/// <summary>
/// Encodes a form
/// </summary>
public interface IFormEncoder
{
    /// <summary>
    /// Encodes a form
    /// </summary>
    /// <param name="form"></param>
    /// <returns></returns>
    public Stream Encode(IFormCollection form);
}