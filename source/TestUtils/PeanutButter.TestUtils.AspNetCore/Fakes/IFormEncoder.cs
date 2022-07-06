using System.IO;
using Microsoft.AspNetCore.Http;

namespace PeanutButter.TestUtils.AspNetCore.Fakes;

public interface IFormEncoder
{
    public Stream Encode(IFormCollection form);
}