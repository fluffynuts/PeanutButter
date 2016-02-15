using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;

namespace NugetPackageVersionIncrementer.Tests
{
    [TestFixture]
    public class TestNuspecFinder
    {
        [Test]
        public void Type_ShouldImplement_INuspecFinder()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(NuspecFinder);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldImplement<INuspecFinder>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => Create());

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_ShouldSetNuspecPathsToEmptyCollection()
        {
            //---------------Set up test pack-------------------
            var sut = Create();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = sut.NuspecPaths;

            //---------------Test Result -----------------------
            Assert.IsNotNull(result);
            CollectionAssert.IsEmpty(result);
        }

        [Test]
        public void FindNuspecsUnder_GivenInvalidPath_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            string folder;
            do
            {
                folder = Path.Combine(Path.GetTempPath(), RandomValueGen.GetRandomString(10, 20));
            } while (Directory.Exists(folder));
            var sut = Create();

            //---------------Assert Precondition----------------
            Assert.IsFalse(Directory.Exists(folder));

            //---------------Execute Test ----------------------
            var ex = Assert.Throws<ArgumentException>(() => sut.FindNuspecsUnder(folder));

            //---------------Test Result -----------------------
            StringAssert.Contains("not found", ex.Message);
        }

        [Test]
        public void FindNuspecsUnder_GivenExistingPathWithNoNuspecs_ShouldLeaveNuspecPathsEmpty()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------

            //---------------Test Result -----------------------
            Assert.Fail("Test Not Yet Implemented");
        }




        private static NuspecFinder Create()
        {
            return new NuspecFinder();
        }
    }
}
