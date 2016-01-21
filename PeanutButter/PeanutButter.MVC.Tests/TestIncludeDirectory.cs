using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;

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

        [TestCase("Path")]
        [TestCase("SearchPattern")]
        [TestCase("SearchSubdirectories")]
        public void PropertyShouldBeReadOnly_(string property)
        {
            //---------------Set up test pack-------------------
            var sut = typeof (IncludeDirectory);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldHaveReadOnlyProperty(property);

            //---------------Test Result -----------------------
        }


    }
}