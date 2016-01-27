using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using PeanutButter.RandomGenerators;

namespace PeanutButter.Utils.Tests
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
            Assert.IsNull(result);
        }

        [Test]
        public void ToMD5Sum_OperatingOnByteArray_ShouldReturnCorrectMd5()
        {
            //---------------Set up test pack-------------------
            var input = RandomValueGen.GetRandomBytes(10);
            var md5 = System.Security.Cryptography.MD5.Create();
            var hash = md5.ComputeHash(input);

            var characters = hash.Select(t => t.ToString("X2")).ToList();
            var expected = string.Join(string.Empty, characters.ToArray());
            

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.ToMD5String();

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
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
            Assert.IsNull(result);
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
            Assert.AreEqual(string.Empty, result);
        }



        [Test]
        public void ToUTF8String_GivenByteArrayWhichIsAString_ShouldReturnCorrectResult()
        {
            //---------------Set up test pack-------------------
            var expected = RandomValueGen.GetRandomString(10, 20);
            var asBytes = Encoding.UTF8.GetBytes(expected);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = asBytes.ToUTF8String();

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }

    }
}
