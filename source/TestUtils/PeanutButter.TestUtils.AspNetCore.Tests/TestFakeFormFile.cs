using System.IO;
using System.Linq;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using static NExpect.Expectations;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.AspNetCore.Tests;

[TestFixture]
public class TestFakeFormFile
{
    [TestFixture]
    public class ParameterlessConstructor
    {
        [Test]
        public void ShouldConstructWithNoData()
        {
            // Arrange
            // Act
            var sut = new FakeFormFile();

            // Assert
            Expect(sut.Length)
                .To.Equal(0);
            Expect(sut.ContentType)
                .To.Equal("application/octet-stream");
        }
    }

    [TestFixture]
    public class ConstructingWithStringDataAndNamesOnly
    {
        [TestCase("text/plain")]
        public void ShouldSetContentTypeTo_(string expected)
        {
            // Arrange
            // Act
            var sut = new FakeFormFile(
                GetRandomString(),
                GetRandomString(),
                GetRandomString()
            );
            // Assert
            Expect(sut.ContentType)
                .To.Equal(expected);
        }
    }

    [TestFixture]
    public class ConstructingWithBinaryDataAndNamesOnly
    {
        [TestCase("text/plain")]
        public void ShouldSetContentTypeTo_(string expected)
        {
            // Arrange
            // Act
            var sut = new FakeFormFile(
                GetRandomString(),
                GetRandomString(),
                GetRandomString()
            );
            // Assert
            Expect(sut.ContentType)
                .To.Equal(expected);
        }
    }

    [TestFixture]
    public class SettingContent
    {
        [Test]
        public void ShouldBeAbleToSetFromStream()
        {
            // Arrange
            var sut = new FakeFormFile();
            var expected = new MemoryStream(GetRandomBytes());
            Expect(sut.Length)
                .To.Equal(0);
            // Act
            sut.SetContent(expected);
            // Assert
            Expect(sut.Content.ToArray())
                .To.Equal(expected.ToArray());
            Expect(sut.Content)
                .Not.To.Be(expected, "should duplicate data, not re-use the reference");
        }
        
        [Test]
        public void ShouldBeAbleToSetFromBytes()
        {
            // Arrange
            var sut = new FakeFormFile();
            var expected = GetRandomBytes();
            Expect(sut.Length)
                .To.Equal(0);
            // Act
            sut.SetContent(expected);
            // Assert
            Expect(sut.Content.ToArray())
                .To.Equal(expected);
            Expect(sut.Content)
                .Not.To.Be(expected, "should duplicate data, not re-use the reference");
        }
        
        [Test]
        public void ShouldBeAbleToSetFromString()
        {
            // Arrange
            var sut = new FakeFormFile();
            var expected = GetRandomString();
            Expect(sut.Length)
                .To.Equal(0);
            // Act
            sut.SetContent(expected);
            // Assert
            Expect(sut.Content.ToArray().AsString())
                .To.Equal(expected);
            Expect(sut.Content)
                .Not.To.Be(expected, "should duplicate data, not re-use the reference");
        }

        [Test]
        public void ShouldBeAbleToSetContentStream()
        {
            // perhaps there's a use-case for allowing overwriting the stream
            // where the caller has a lazy eval or something 🤷
            // Arrange
            var sut = new FakeFormFile();
            var data = GetRandomBytes();
            var newContent = new MyStream(data);
            
            // Act
            sut.SetContentStream(newContent);
            // Assert
            Expect(sut.Content)
                .To.Equal(data);
            Expect(newContent.CopyCount)
                .To.Equal(1);
        }
    }

    public class MyStream : MemoryStream
    {
        public int CopyCount { get; private set; }

        public MyStream(byte[] data) : base(data)
        {
        }

        public override void CopyTo(Stream destination, int bufferSize)
        {
            CopyCount++;
            base.CopyTo(destination, bufferSize);
        }
    }

}