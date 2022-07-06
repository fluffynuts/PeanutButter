using System.IO;
using Microsoft.AspNetCore.Http;

namespace PeanutButter.TestUtils.AspNetCore.Fakes;

public class NullBodyEncoder : IFormEncoder
{
    public Stream Encode(IFormCollection form)
    {
        return new MemoryStream();
    }
}