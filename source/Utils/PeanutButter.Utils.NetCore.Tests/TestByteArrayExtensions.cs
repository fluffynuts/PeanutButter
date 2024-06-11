using System.IO.Compression;
using System.Text;

// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable ExpressionIsAlwaysNull

namespace PeanutButter.Utils.Tests;

[TestFixture]
public class TestByteArrayExtensions
{
    [TestFixture]
    public class ToMd5Sum
    {
        [TestFixture]
        public class GivenNullArray
        {
            [Test]
            public void ShouldReturnNull()
            {
                //---------------Set up test pack-------------------
                byte[] input = null;

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = input.ToMd5String();

                //---------------Test Result -----------------------
                Expect(result).To.Be.Null();
            }
        }

        [TestFixture]
        public class OperatingOnByteArray
        {
            [Test]
            public void ShouldReturnCorrectMd5()
            {
                //---------------Set up test pack-------------------
                var input = GetRandomBytes(10);
                var md5 = System.Security.Cryptography.MD5.Create();
                var hash = md5.ComputeHash(input);

                var characters = hash.Select(t => t.ToString("X2")).ToList();
                var expected = string.Join(string.Empty, characters.ToArray());


                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = input.ToMd5String();

                //---------------Test Result -----------------------
                Expect(result).To.Equal(expected);
            }
        }
    }

    [TestFixture]
    public class ToUtf8String
    {
        [TestFixture]
        public class GivenNull
        {
            [Test]
            public void ShouldReturnNull()
            {
                //---------------Set up test pack-------------------
                byte[] input = null;

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = input.ToUtf8String();

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Be.Null();
            }
        }

        [TestFixture]
        public class GivenEmptyArray
        {
            [Test]
            public void ShouldReturnEmptyString()
            {
                //---------------Set up test pack-------------------
                var input = Array.Empty<byte>();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = input.ToUtf8String();

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Be.Empty();
            }
        }


        [TestFixture]
        public class GivenByteArrayWhichIsAString
        {
            [Test]
            public void ShouldReturnCorrectResult()
            {
                //---------------Set up test pack-------------------
                var expected = GetRandomString(10, 20);
                var asBytes = Encoding.UTF8.GetBytes(expected);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = asBytes.ToUtf8String();

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Equal(expected);
            }
        }
    }

    [TestFixture]
    public class Base64Conversions
    {
        [Test]
        public void ToBase64_ShouldReturnBase64StringForBytes()
        {
            //--------------- Arrange -------------------
            var input = GetRandomCollection<byte>(5, 10).ToArray();
            var expected = Convert.ToBase64String(input);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = input.ToBase64();

            //--------------- Assert -----------------------
            Expect(result)
                .To.Equal(expected);
        }
    }

    [TestFixture]
    public class ToMemoryStream
    {
        [Test]
        public void ShouldConvertNonEmptyByteArray()
        {
            // Arrange
            var input = GetRandomBytes(10);
            // Act
            var result = input.ToMemoryStream();
            // Assert
            Expect(result.ToArray())
                .To.Equal(input);
        }

        [Test]
        public void ShouldConvertEmptyByteArray()
        {
            // Arrange
            var input = new byte[0];
            // Act
            var result = input.ToMemoryStream();
            // Assert
            Expect(result.ToArray())
                .To.Be.Empty();
        }

        [Test]
        public void ShouldTreatNullAsEmpty()
        {
            // Arrange
            var input = null as byte[];
            // Act
            var result = input.ToMemoryStream();
            // Assert
            Expect(result.ToArray())
                .To.Be.Empty();
        }
    }

    [TestFixture]
    public class StartsWith
    {
        [TestFixture]
        public class WhenEitherIsNull
        {
            [Test]
            public void ShouldThrow1()
            {
                // Arrange
                var data = null as byte[];
                var compare = GetRandomArray<byte>();
                // Act
                Expect(() => data.StartsWith(compare))
                    .To.Throw<ArgumentNullException>()
                    .For("data");
                // Assert
            }

            [Test]
            public void ShouldThrow2()
            {
                // Arrange
                var compare = null as byte[];
                var data = GetRandomArray<byte>();
                // Act
                Expect(() => data.StartsWith(compare))
                    .To.Throw<ArgumentNullException>()
                    .For("reference");
                // Assert
            }
        }

        [TestFixture]
        public class WhenDataIsTooShort
        {
            [Test]
            public void ShouldReturnFalse()
            {
                // Arrange
                var data = new byte[] { 0x0, 0x1, 0x2 };
                var compare = new byte[] { 0x0, 0x1, 0x2, 0x3 };
                // Act
                var result = data.StartsWith(compare);
                // Assert
                Expect(result)
                    .To.Be.False();
            }
        }

        [TestFixture]
        public class WhenDataDoesNotStartWithReference
        {
            [Test]
            public void ShouldReturnFalse()
            {
                // Arrange
                var data = new byte[] { 0x0, 0x1, 0x2 };
                var compare = new byte[] { 0x1, 0x2 };
                // Act
                var result = data.StartsWith(compare);
                // Assert
                Expect(result)
                    .To.Be.False();
            }
        }

        [TestFixture]
        public class WhenDataEqualsReference
        {
            [Test]
            public void ShouldReturnTrue()
            {
                // Arrange
                var data = GetRandomArray<byte>(5, 10);
                var compare = data.ToArray();
                Expect(data)
                    .Not.To.Be(compare);
                Expect(data)
                    .To.Equal(compare);
                // Act
                var result = data.StartsWith(compare);
                // Assert
                Expect(result)
                    .To.Be.True();
            }
        }

        [TestFixture]
        public class WhenDataIsLongerAndStartsWithReference
        {
            [Test]
            public void ShouldReturnTrue()
            {
                // Arrange
                var data = GetRandomArray<byte>(5, 10);
                var compare = data.Take(4).ToArray();
                
                // Act
                var result = data.StartsWith(compare);
                // Assert
                Expect(result)
                    .To.Be.True();
            }
        }
    }

    [TestFixture]
    public class GZip
    {
        [Test]
        public void ShouldGzipTheData()
        {
            // Arrange
            var data = GetRandomBytes();
            using var source = new MemoryStream(data);
            using var target = new MemoryStream();
            using var gzip = new GZipStream(target, CompressionLevel.Optimal, leaveOpen: true);
            source.CopyTo(gzip);
            gzip.Close();
            var expected = target.ToArray();
            // Act
            var result = data.GZip();
            // Assert
            Expect(result)
                .To.Equal(expected);
        }
    }

    [TestFixture]
    public class UnGZip
    {
        [Test]
        public void ShouldDecompressTheData()
        {
            // Arrange
            var data = GetRandomBytes();
            var compressed = data.GZip();
            
            // Act
            var result = compressed.UnGZip();
            // Assert
            Expect(result)
                .To.Equal(data);
        }
    }
}