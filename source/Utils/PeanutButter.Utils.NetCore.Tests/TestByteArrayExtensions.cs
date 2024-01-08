using System.Text;
using static PeanutButter.RandomGenerators.RandomValueGen;

// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable ExpressionIsAlwaysNull

namespace PeanutButter.Utils.NetCore.Tests
{
    [TestFixture]
    public class TestByteArrayExtensions
    {
        [Test]
        public void ToMD5Sum_GivenNullArray_ShouldReturnNull()
        {
            //---------------Set up test pack-------------------
            byte[] input = null;

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.ToMD5String();

            //---------------Test Result -----------------------
            Expect(result).To.Be.Null();
        }

        [Test]
        public void ToMD5Sum_OperatingOnByteArray_ShouldReturnCorrectMd5()
        {
            //---------------Set up test pack-------------------
            var input = GetRandomBytes(10);
            var md5 = System.Security.Cryptography.MD5.Create();
            var hash = md5.ComputeHash(input);

            var characters = hash.Select(t => t.ToString("X2")).ToList();
            var expected = string.Join(string.Empty, characters.ToArray());


            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.ToMD5String();

            //---------------Test Result -----------------------
            Expect(result).To.Equal(expected);
        }

        [Test]
        public void ToUTF8String_GivenNull_ShouldReturnNull()
        {
            //---------------Set up test pack-------------------
            byte[] input = null;

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.ToUTF8String();

            //---------------Test Result -----------------------
            Expect(result).To.Be.Null();
        }

        [Test]
        public void ToUTF8String_GivenEmptyArray_ShouldReturnEmptyString()
        {
            //---------------Set up test pack-------------------
            var input = new byte[] {};

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.ToUTF8String();

            //---------------Test Result -----------------------
            Expect(result).To.Be.Empty();
        }



        [Test]
        public void ToUTF8String_GivenByteArrayWhichIsAString_ShouldReturnCorrectResult()
        {
            //---------------Set up test pack-------------------
            var expected = GetRandomString(10, 20);
            var asBytes = Encoding.UTF8.GetBytes(expected);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = asBytes.ToUTF8String();

            //---------------Test Result -----------------------
            Expect(result).To.Equal(expected);
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
                Expect(result).To.Equal(expected);
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
                Expect(result.ToArray()).To.Equal(input);
            }

            [Test]
            public void ShouldConvertEmptyByteArray()
            {
                // Arrange
                var input = new byte[0];
                // Act
                var result = input.ToMemoryStream();
                // Assert
                Expect(result.ToArray()).To.Be.Empty();
            }

            [Test]
            public void ShouldTreatNullAsEmpty()
            {
                // Arrange
                var input = null as byte[];
                // Act
                var result = input.ToMemoryStream();
                // Assert
                Expect(result.ToArray()).To.Be.Empty();
            }
        }
    }
}
