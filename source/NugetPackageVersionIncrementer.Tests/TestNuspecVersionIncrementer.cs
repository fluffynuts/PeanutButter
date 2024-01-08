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
            Expect(() => new NuspecVersionIncrementer(xml, ""))
                .To.Throw<ArgumentException>()
                .With.Message.Containing("not valid xml");

            //---------------Test Result -----------------------
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
            Expect(result).To.Equal(expected);
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
            Expect(sut.Version).To.Equal(initial);

            //---------------Execute Test ----------------------
            sut.IncrementMinorVersion();

            //---------------Test Result -----------------------
            Expect(sut.Version).To.Equal(expected);
        }

        private NuspecVersionIncrementer Create(
            string input
        )
        {
            return new NuspecVersionIncrementer(input, null);
        }
    }
}