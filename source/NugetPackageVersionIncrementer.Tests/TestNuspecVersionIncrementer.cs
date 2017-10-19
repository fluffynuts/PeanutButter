using System;
using NUnit.Framework;
// ReSharper disable ObjectCreationAsStatement

namespace NugetPackageVersionIncrementer.Tests
{
    [TestFixture]
    public class TestNuspecVersionIncrementer
    {
        [TestCase("")]
        [TestCase(null)]
        [TestCase(" ")]
        [TestCase("\r\n\t")]
        public void Construct_GivenObviouslyInvalidXml_ShouldThrow(string xml)
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ex = Assert.Throws<ArgumentException>(() => new NuspecVersionIncrementer(xml));

            //---------------Test Result -----------------------
            StringAssert.Contains("not valid xml", ex.Message.ToLower());
        }

        [Test]
        public void Construct_GivenValidXml_ShouldSetUpVersion()
        {
            //---------------Set up test pack-------------------
            var expected = "1.1.33";
            var input = TestResources.package1;
            var sut = Create(input);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = sut.Version;

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }


        [Test]
        public void IncrementMinorPackageVersion_ShouldIncrementMinorPackageVersion()
        {
            //---------------Set up test pack-------------------
            var initial = "1.1.33";
            var expected = "1.1.34";
            var input = TestResources.package1;
            var sut = Create(input);

            //---------------Assert Precondition----------------
            Assert.AreEqual(initial, sut.Version);

            //---------------Execute Test ----------------------
            sut.IncrementMinorVersion();

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, sut.Version);
        }

        private NuspecVersionIncrementer Create(string input)
        {
            return new NuspecVersionIncrementer(input);
        }
    }
}
