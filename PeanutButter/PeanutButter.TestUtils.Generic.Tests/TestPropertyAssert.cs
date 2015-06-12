using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.Generic.Tests
{
    [TestFixture]
    public class TestPropertyAssert
    {
        [Test]
        public void AreEqual_GivenTwoDifferentTypedObjects_AndPropertyNames_WhenPropertiesMatch_DoesNotThrow()
        {
            //---------------Set up test pack-------------------
            var obj1 = new { foo = "foo" };
            var obj2 = new { bar = "foo" };

            //---------------Assert Precondition----------------
            Assert.AreNotEqual(obj1, obj2);
            Assert.AreNotEqual(obj1.GetType(), obj2.GetType());
            //---------------Execute Test ----------------------

            Assert.DoesNotThrow(() => PropertyAssert.AreEqual(obj1, obj2, "foo", "bar"));

            //---------------Test Result -----------------------
        }

        [Test]
        public void AreEqual_GivenTwoObjectsWithNonMatchingPropertyValues_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var obj1 = new { foo = "foo" };
            var obj2 = new { bar = "bar" };
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<AssertionException>(() => PropertyAssert.AreEqual(obj1, obj2, "foo", "bar"));

            //---------------Test Result -----------------------
        }

        [Test]
        public void AreEqual_GivenTwoObjectsWithNonMatchingPropertyTypes_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var obj1 = new { foo = "1" };
            var obj2 = new { bar = 1 };
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<AssertionException>(() => PropertyAssert.AreEqual(obj1, obj2, "foo", "bar"));

            //---------------Test Result -----------------------
        }

        [Test]
        public void AreEqual_GivenTwoDifferentTypedObjects_AndJustOnePropertyName_ComparesTheSameNamedPropertyOnBoth()
        {
            //---------------Set up test pack-------------------
            var obj1 = new { foo = "foo" };
            var obj2 = new { bar = "bar", foo = "foo" };
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => PropertyAssert.AreEqual(obj1, obj2, "foo"));

            //---------------Test Result -----------------------
        }

        [Test]
        public void AreEqual_WhenPropertyNotFoundByName_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var obj1 = new { foo = "foo" };
            var obj2 = new { bar = "bar", foo = "foo" };
            var badName = "foo1";
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ex = Assert.Throws<AssertionException>(() => PropertyAssert.AreEqual(obj1, obj2, badName));

            //---------------Test Result -----------------------
            StringAssert.Contains("Unable to find property", ex.Message);
            StringAssert.Contains(badName, ex.Message);
        }

        [Test]
        public void AllPropertiesAreEqual_ShouldCompareAllPropertiesAndNotThrowWhenTheyAllMatch()
        {
            //---------------Set up test pack-------------------
            var v1 = RandomValueGen.GetRandomString();
            var v2 = RandomValueGen.GetRandomInt();
            var v3 = RandomValueGen.GetRandomDate();

            var obj1 = new { v1 = v1, v2 = v2, v3 = v3 };
            var obj2 = new { v1 = v1, v2 = v2, v3 = v3 };
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => PropertyAssert.AllPropertiesAreEqual(obj1, obj2));

            //---------------Test Result -----------------------
        }

        [Test]
        public void AllPropertiesAreEqual_WhenOneObjectHasExtraProperties_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var v1 = RandomValueGen.GetRandomString();
            var v2 = RandomValueGen.GetRandomInt();
            var v3 = RandomValueGen.GetRandomDate();

            var obj1 = new { v1 = v1, v2 = v2, v3 = v3 };
            var obj2 = new { v1 = v1, v2 = v2, v3 = v3, v4 = RandomValueGen.GetRandomBoolean() };
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<AssertionException>(() => PropertyAssert.AllPropertiesAreEqual(obj1, obj2));

            //---------------Test Result -----------------------
        }

        [Test]
        public void MatchingPropertiesAreEqual_WhenGivenTwoObjects_ShouldCompareSameNamedPropertiesAndNotThrowIfTheyMatch()
        {
            //---------------Set up test pack-------------------
            var v1 = RandomValueGen.GetRandomString();
            var v2 = RandomValueGen.GetRandomInt();
            var v3 = RandomValueGen.GetRandomDate();

            var obj1 = new { v1 = v1, v2 = v2, v3 = v3 };
            var obj2 = new { v1 = v1, v2 = v2, v3 = v3, v4 = RandomValueGen.GetRandomBoolean() };
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => PropertyAssert.MatchingPropertiesAreEqual(obj1, obj2));

            //---------------Test Result -----------------------
        }

        [Test]
        public void MatchingPropertiesAreEqual_WhenGivenTwoObjects_ShouldCompareSameNamedPropertiesAndThrowIfTheyDoNotMatchValue()
        {
            //---------------Set up test pack-------------------
            var v1 = RandomValueGen.GetRandomString();
            var v2 = RandomValueGen.GetRandomInt();
            var v3 = RandomValueGen.GetRandomDate();

            var obj1 = new { v1 = v1, v2 = v2, v3 = v3 };
            var obj2 = new { v1 = v1 + " ", v2 = v2, v3 = v3, v4 = RandomValueGen.GetRandomBoolean() };
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<AssertionException>(() => PropertyAssert.MatchingPropertiesAreEqual(obj1, obj2));

            //---------------Test Result -----------------------
        }

        [Test]
        public void MatchingPropertiesAreEqual_WhenGivenTwoObjects_ShouldCompareSameNamedPropertiesAndThrowIfTheyDoNotMatchType()
        {
            //---------------Set up test pack-------------------
            var v1 = RandomValueGen.GetRandomString();
            var v2 = RandomValueGen.GetRandomInt();
            var v3 = RandomValueGen.GetRandomDate();

            var obj1 = new { v1 = v1, v2 = v2, v3 = v3 };
            var obj2 = new { v1 = v1, v2 = Decimal.Parse(v2.ToString()), v3 = v3, v4 = RandomValueGen.GetRandomBoolean() };
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<AssertionException>(() => PropertyAssert.MatchingPropertiesAreEqual(obj1, obj2));

            //---------------Test Result -----------------------
        }

        public class BaseClass
        {
            public virtual string Name { get; set; }
        }
        public class DerivedClass: BaseClass
        {
            public override string Name
            {
                get
                {
                    return "Some constant";
                }
            }
        }

        private void DoTest(BaseClass b1, BaseClass b2)
        {
            PropertyAssert.AllPropertiesAreEqual(b1, b2);
        }

        [Test]
        public void AreEqual_IsNotConfusedByOverridingProperties()
        {
            //---------------Set up test pack-------------------
            var d1 = new DerivedClass();
            var d2 = new BaseClass() { Name = "Some constant" };
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => DoTest(d1, d2));

            //---------------Test Result -----------------------
        }

        [Test]
        public void AllPropertiesAreEqua_WhenGivenIgnoreList_ShouldHonorThatList()
        {
            //---------------Set up test pack-------------------
            var d1 = new
            {
                prop = "prop",
                ignoreMe = 1
            };
            var d2 = new
            {
                prop = d1.prop,
                ignoreMe = d1.ignoreMe + 1
            };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => d1.AllPropertiesMatch(d2, "ignoreMe"));

            //---------------Test Result -----------------------
        }

    }

}
