using NUnit.Framework;
using PeanutButter.RandomGenerators;

namespace PeanutButter.MVC.Tests
{
    [TestFixture]
    public class TestIncludeDirectory
    {
        [Test]
        public void Construct_ShouldCopyParametersToProperties()
        {
            //---------------Set up test pack-------------------
            var expectedPath = RandomValueGen.GetRandomString();
            var expectedPattern = RandomValueGen.GetRandomString();
            var expectedSubdirectories = RandomValueGen.GetRandomBoolean();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = new IncludeDirectory(expectedPath, expectedPattern, expectedSubdirectories);

            //---------------Test Result -----------------------
            Assert.AreEqual(expectedPath, sut.Path);
            Assert.AreEqual(expectedPattern, sut.SearchPattern);
            Assert.AreEqual(expectedSubdirectories, sut.SearchSubdirectories);
        }

    }
}