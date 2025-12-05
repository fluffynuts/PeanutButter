using System.IO;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;
using NExpect;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.SimpleHTTPServer.Tests;

[TestFixture]
public class TestStreamExtensions
{
    public class SomePoco
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }

    [Test]
    public void ShouldBeAbleToDeserializeStream()
    {
        // Arrange
        var src = new SomePoco() { Id = 2, Value = "some value" };
        var srcJson = JsonConvert.SerializeObject(src);
        var memStream = new MemoryStream(Encoding.UTF8.GetBytes(srcJson));
        // Act
        var result = memStream.As<SomePoco>();
        // Assert
        Expect(result).To.Deep.Equal(src);
    }

    [Test]
    public void ShouldConvertPureString()
    {
        // Arrange
        var str = GetRandomString(50);
        var bytes = Encoding.UTF8.GetBytes(str);
        using var memStream = new MemoryStream(bytes);
        // Act
        var result = memStream.As<string>();
        // Assert
        Expect(result)
            .To.Equal(str);
    }
}