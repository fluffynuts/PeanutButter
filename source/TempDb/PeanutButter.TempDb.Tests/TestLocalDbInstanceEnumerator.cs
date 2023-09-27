using System;
using NExpect;
using NUnit.Framework;
using PeanutButter.TempDb.LocalDb;
using PeanutButter.Utils;
using static NExpect.Expectations;

namespace PeanutButter.TempDb.Tests
{
    [TestFixture]
    public class TestLocalDbInstanceEnumerator
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            if (!Platform.IsWindows)
            {
                Assert.Ignore(
                    $"{nameof(LocalDbInstanceEnumerator)} is only available on win32"
                );
            }
        }

        [Test]
        public void InstanceFinder()
        {
            //---------------Set up test pack-------------------
            var sut = new LocalDbInstanceEnumerator();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = sut.FindInstances();

            //---------------Test Result -----------------------
            Console.WriteLine($"LocalDbInstanceFinder finds:\n{string.Join("\n", result)}");
            Expect(result)
                .Not.To.Be.Empty(
                    () => "If this utility can't find a v-instance of localdb, other tests are going to cry"
                );
        }

        [Test]
        [Explicit("Works on my machine")]
        public void InstanceFinder_ShouldFindNewInstanceName()
        {
            //---------------Set up test pack-------------------
            var sut = new LocalDbInstanceEnumerator();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = sut.FindInstances();

            //---------------Test Result -----------------------
            Expect(result).Not.To.Contain("MSSQLLocalDB");
        }
    }
}