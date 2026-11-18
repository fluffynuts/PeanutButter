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
            Expect(sut.Path)
                .To.Equal(expectedPath);
            Expect(sut.SearchPattern)
                .To.Equal(expectedPattern);
            Expect(sut.SearchSubdirectories)
                .To.Equal(expectedSubdirectories);
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
            Expect(() => sut.ShouldHaveReadOnlyProperty(property))
                .Not.To.Throw();

            //---------------Test Result -----------------------
        }
    }
}