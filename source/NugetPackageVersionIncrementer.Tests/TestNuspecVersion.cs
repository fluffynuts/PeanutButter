using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using PeanutButter.RandomGenerators;

namespace NugetPackageVersionIncrementer.Tests
{
    [TestFixture]
    public class TestNuspecVersion
    {
        [Test]
        public void Construct_GivenEmptyString_ShouldSetMinAndMaxVersionsEmpty()
        {
            //---------------Set up test pack-------------------

            //---------------Set up test pack-------------------

            //---------------Execute Test ----------------------
            var sut = Create("");

            //---------------Test Result -----------------------
            Assert.AreEqual("", sut.Minimum);
            Assert.AreEqual("", sut.Maximum);
        }

        [Test]
        public void Construct_GivenPlainVersion_ShouldSetMinimumAndMaximumToVersion()
        {
            //---------------Set up test pack-------------------
            var versionString = GetRandomVersionString();

            //---------------Set up test pack-------------------

            //---------------Execute Test ----------------------
            var sut = Create(versionString);

            //---------------Test Result -----------------------
            Assert.AreEqual(versionString, sut.Minimum);
            Assert.AreEqual(versionString, sut.Maximum);
        }

        [Test]
        public void Construct_GivenVersionRange_ShouldSetMinimumAndMaximum()
        {
            //---------------Set up test pack-------------------
            var min = GetRandomVersionString();
            var max = GetRandomVersionString();
            var version = $"[{min},{max}]";

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = Create(version);

            //---------------Test Result -----------------------
            Assert.AreEqual(min, sut.Minimum);
            Assert.AreEqual(max, sut.Maximum);

        }

        // unsurprising tests, but they define the expected result for missing one version string
        [Test]
        public void Construct_GivenVersionRange_WhenHaveOnlyMinimum_ShouldSetMinimumVersionAndMaxAsEmptyString()
        {
            //---------------Set up test pack-------------------
            var min = GetRandomVersionString();
            var max = "";
            var version = $"[{min},{max}]";

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = Create(version);

            //---------------Test Result -----------------------
            Assert.AreEqual(min, sut.Minimum);
            Assert.AreEqual(max, sut.Maximum);

        }


        [Test]
        public void Construct_GivenVersionRange_WhenHaveOnlyMaximum_ShouldSetMaximumVersionAndMinAsEmptyString()
        {
            //---------------Set up test pack-------------------
            var min = "";
            var max = GetRandomVersionString();
            var version = $"[{min},{max}]";

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = Create(version);

            //---------------Test Result -----------------------
            Assert.AreEqual(min, sut.Minimum);
            Assert.AreEqual(max, sut.Maximum);

        }


        [Test]
        public void ToString_WhenMinimumSet_ShouldProduceExpectedString()
        {
            //---------------Set up test pack-------------------
            var min = GetRandomVersionString();
            var expected = $"[{min},]";
            var sut = Create("");

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.Minimum = min;
            var result = sut.ToString();

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);

        }

        [Test]
        public void ToString_WhenMaximumSet_ShouldProduceExpectedString()
        {
            //---------------Set up test pack-------------------
            var max = GetRandomVersionString();
            var expected = $"[,{max}]";
            var sut = Create("");

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.Maximum = max;
            var result = sut.ToString();

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);

        }

        [Test]
        public void ToString_WhenMinimumAndMaximumSetToDifferentValues_ShouldProduceExpectedString()
        {
            //---------------Set up test pack-------------------
            var min = GetRandomVersionString();
            var max = GetRandomVersionString();
            while (min == max)
                max = GetRandomVersionString();
            var expected = $"[{min},{max}]";
            var sut = Create("");

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.Minimum = min;
            sut.Maximum = max;
            var result = sut.ToString();

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);

        }

        [Test]
        public void ToString_WhenMinimumAndMaximumSetToSameValues_ShouldProduceExpectedString()
        {
            //---------------Set up test pack-------------------
            var version = GetRandomVersionString();
            var expected = version;
            var sut = Create("");

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.Minimum = version;
            sut.Maximum = version;
            var result = sut.ToString();

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);

        }

        private static string GetRandomVersionString()
        {
            return string.Join(".", RandomValueGen.GetRandomCollection<int>(3,3));
        }


        private NuspecVersion Create(string versionString)
        {
            return new NuspecVersion(versionString);
        }
    }
}
