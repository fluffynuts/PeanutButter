using System.IO;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;
using NExpect;
using static NExpect.Expectations;

namespace PeanutButter.SimpleHTTPServer.Tests
{
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
    }
}