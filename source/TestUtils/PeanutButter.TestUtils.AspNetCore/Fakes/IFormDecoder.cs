using System.IO;
using Microsoft.AspNetCore.Http;

namespace PeanutButter.TestUtils.AspNetCore.Fakes;

public interface IFormDecoder
{
    public IFormCollection Decode(Stream body);
}