using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestDeepEqualityTester: AssertionHelper
    {
        // mostly this class is tested through the DeepEquals()
        //  extension method testing. However, I'd like to allow
        //  for a slower operation where discrepencies are recorded
        [Test]
        public void AreDeepEqual_GivenTwoEqualPrimitives_ShouldNotPopulateErrors()
        {
            //--------------- Arrange -------------------
            var sut = Create(1, 1);

            //--------------- Assume ----------------
            Expect(sut.Errors, Is.Empty);

            //--------------- Act ----------------------
            Expect(sut.AreDeepEqual(), Is.True);

            //--------------- Assert -----------------------
            Expect(sut.Errors, Is.Empty);
        }

        [Test]
        public void AreDeepEqual_GivenTwoDifferentPrimitives_ShouldSetExpectedError()
        {
            //--------------- Arrange -------------------
            var sut = Create(true, false);
            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(sut.AreDeepEqual(), Is.False);

            //--------------- Assert -----------------------
            Expect(sut.Errors, Does.Contain("Primitive values differ"));
        }

        [Test]
        public void AreDeepEqual_GivenDifferingComplexObjectsWithOnePropertyOfSameNameAndValue_ShouldRecordError()
        {
            //--------------- Arrange -------------------
            var item1 = new { foo = 1 };
            var item2 = new { foo = 2 };
            var sut = Create(item1, item2);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(sut.AreDeepEqual(), Is.False);

            //--------------- Assert -----------------------
            var error = sut.Errors.Single();
            Expect(error, Does.Contain("foo"));
            Expect(error, Does.Contain("1"));
            Expect(error, Does.Contain("2"));
        }

        [Test]
        [Ignore("WIP: for now, short-circuit is ok; I'd like to use this in PropertyAssert though, which attempts to be more explicit about failures")]
        public void AreDeepEqual_WhenReportingIsEnabled_ShouldNotShortCircuitTests()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------

            //--------------- Assert -----------------------
            Assert.Fail("Test Not Yet Implemented");
        }

        private DeepEqualityTester Create(object obj1, object obj2)
        {
            var sut = new DeepEqualityTester(obj1, obj2);
            sut.RecordErrors = true;
            return sut;
        }
    }
}
