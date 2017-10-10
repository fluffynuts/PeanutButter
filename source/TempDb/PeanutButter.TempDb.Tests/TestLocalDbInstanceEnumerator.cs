using System;
using NExpect;
using NUnit.Framework;
using PeanutButter.TempDb.LocalDb;
using static NExpect.Expectations;

namespace PeanutButter.TempDb.Tests
{
    [TestFixture]
    public class TestLocalDbInstanceEnumerator
    {
        [Test]
        public void InstanceFinder()
        {
            //---------------Set up test pack-------------------
            var sut = new LocalDbInstanceEnumerator();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = sut.FindInstances();

            //---------------Test Result -----------------------
            Console.WriteLine(string.Join("\n", result));
            CollectionAssert.IsNotEmpty(result, "If this utility can't find a v-instance of localdb, other tests are going to cry");
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