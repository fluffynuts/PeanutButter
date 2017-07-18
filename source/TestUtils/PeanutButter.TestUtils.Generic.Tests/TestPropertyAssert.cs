using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using PeanutButter.DuckTyping;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TestUtils.Generic.Tests
{
    [TestFixture]
    public class TestPropertyAssert: AssertionHelper
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
        public void AreNotEqual_GivenTwoDifferentTypedObjects_AndPropertyNames_WhenPropertiesMatch_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var obj1 = new { foo = "foo" };
            var obj2 = new { bar = "foo" };

            //---------------Assert Precondition----------------
            Assert.AreNotEqual(obj1, obj2);
            Assert.AreNotEqual(obj1.GetType(), obj2.GetType());
            //---------------Execute Test ----------------------

            Assert.Throws<AssertionException>(() => PropertyAssert.AreNotEqual(obj1, obj2, "foo", "bar"));

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
        public void AreNotEqual_GivenTwoObjectsWithNonMatchingPropertyValues_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var obj1 = new { foo = "foo" };
            var obj2 = new { bar = "bar" };
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => PropertyAssert.AreNotEqual(obj1, obj2, "foo", "bar"));

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
        public void AreNotEqual_GivenTwoObjectsWithNonMatchingPropertyTypes_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var obj1 = new { foo = "1" };
            var obj2 = new { bar = 1 };
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<AssertionException>(() => PropertyAssert.AreNotEqual(obj1, obj2, "foo", "bar"));

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
        public void AreNotEqual_GivenTwoDifferentTypedObjects_AndJustOnePropertyName_ComparesTheSameNamedPropertyOnBoth()
        {
            //---------------Set up test pack-------------------
            var obj1 = new { foo = "foo" };
            var obj2 = new { bar = "bar", foo = "foo1" };
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => PropertyAssert.AreNotEqual(obj1, obj2, "foo"));

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
        public void AreNotEqual_WhenPropertyNotFoundByName_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var obj1 = new { foo = "foo" };
            var obj2 = new { bar = "bar", foo = "foo" };
            var badName = "foo1";
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ex = Assert.Throws<AssertionException>(() => PropertyAssert.AreNotEqual(obj1, obj2, badName));

            //---------------Test Result -----------------------
            StringAssert.Contains("Unable to find property", ex.Message);
            StringAssert.Contains(badName, ex.Message);
        }

        [Test]
        public void AreEqual_ShouldBeAbleToFollowPropertyPaths()
        {
            //---------------Set up test pack-------------------
            var obj1 = new {foo = new {name = "bar"}};
            var obj2 = new {foo = new {name = "bar"}};

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => PropertyAssert.AreEqual(obj1, obj2, "foo.name"));

            //---------------Test Result -----------------------
        }

        [Test]
        public void AreNotEqual_ShouldBeAbleToFollowPropertyPaths()
        {
            //---------------Set up test pack-------------------
            var obj1 = new {foo = new {name = "bar"}};
            var obj2 = new {foo = new {name = "quuz"}};

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => PropertyAssert.AreNotEqual(obj1, obj2, "foo.name"));

            //---------------Test Result -----------------------
        }


        [Test]
        public void AllPropertiesAreEqual_ShouldCompareAllPropertiesAndNotThrowWhenTheyAllMatch()
        {
            //---------------Set up test pack-------------------
            var v1 = GetRandomString();
            var v2 = GetRandomInt();
            var v3 = GetRandomDate();

            var obj1 = new {v1, v2, v3 };
            var obj2 = new {v1, v2, v3 };
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => PropertyAssert.AreDeepEqual(obj1, obj2));

            //---------------Test Result -----------------------
        }

        [Test]
        public void AllPropertiesAreEqual_WhenOneObjectHasExtraProperties_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var v1 = GetRandomString();
            var v2 = GetRandomInt();
            var v3 = GetRandomDate();

            var obj1 = new {v1, v2, v3 };
            var obj2 = new {v1, v2, v3, v4 = GetRandomBoolean() };
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<AssertionException>(() => PropertyAssert.AreDeepEqual(obj1, obj2));

            //---------------Test Result -----------------------
        }


        [Test]
        public void AllPropertiesAreEqual_WhenOneObjectHasExtraProperties_ShouldIncludeUsefulInformationInThrownExceptionMessage()
        {
            //---------------Set up test pack-------------------
            var v1 = GetRandomString();
            var v2 = GetRandomInt();
            var v3 = GetRandomDate();

            var obj1 = new {v1, v2, v3 };
            var obj2 = new {v1, v2, v3, v4 = GetRandomBoolean() };
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ex = Assert.Throws<AssertionException>(() => PropertyAssert.AreDeepEqual(obj1, obj2));

            //---------------Test Result -----------------------
            Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
            Console.WriteLine(ex.Message);
        }

        [Test]
        public void AllPropertiesAreEqual_WhenOneObjectHasDifferentlyNamedProperty_ShouldIncludeUsefulInformationInThrownExceptionMessage()
        {
            //---------------Set up test pack-------------------
            var v1 = GetRandomString();
            var v2 = GetRandomInt();
            var v3 = GetRandomDate();

            var obj1 = new {v1, v2, v3 };
            var obj2 = new {v1, v2, v4 = v3 };
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ex = Assert.Throws<AssertionException>(
                () => PropertyAssert.AreDeepEqual(obj1, obj2)
            );

            //---------------Test Result -----------------------
            Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
            StringAssert.Contains("v3", ex.Message);
        }

        [Test]
        public void MatchingPropertiesAreEqual_WhenGivenTwoObjects_ShouldCompareSameNamedPropertiesAndNotThrowIfTheyMatch()
        {
            //---------------Set up test pack-------------------
            var v1 = GetRandomString();
            var v2 = GetRandomInt();
            var v3 = GetRandomDate();

            var obj1 = new {v1, v2, v3 };
            var obj2 = new {v1, v2, v3, v4 = GetRandomBoolean() };
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => PropertyAssert.AreIntersectionEqual(obj1, obj2));

            //---------------Test Result -----------------------
        }

        [Test]
        public void MatchingPropertiesAreEqual_WhenGivenTwoObjects_ShouldCompareSameNamedPropertiesAndThrowIfTheyDoNotMatchValue()
        {
            //---------------Set up test pack-------------------
            var v1 = GetRandomString();
            var v2 = GetRandomInt();
            var v3 = GetRandomDate();

            var obj1 = new {v1, v2, v3 };
            var obj2 = new { v1 = v1 + " ", v2, v3, v4 = GetRandomBoolean() };
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<AssertionException>(() => PropertyAssert.AreIntersectionEqual(obj1, obj2));

            //---------------Test Result -----------------------
        }

        [Test]
        public void MatchingPropertiesAreEqual_WhenGivenTwoObjects_ShouldCompareSameNamedPropertiesAndThrowIfTheyDoNotMatchType()
        {
            //---------------Set up test pack-------------------
            var v1 = GetRandomString();
            var v2 = GetRandomInt();
            var v3 = GetRandomDate();

            var obj1 = new {v1, v2, v3 };
            var obj2 = new {v1, v2 = decimal.Parse(v2.ToString()), v3, v4 = GetRandomBoolean() };
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<AssertionException>(() => PropertyAssert.AreIntersectionEqual(obj1, obj2));

            //---------------Test Result -----------------------
        }

        [Test]
        public void AreDeepEqual_ShouldConsiderTwoListsWithSameContentsAsEqual()
        {
            // Arrange
            var l1 = new List<string>(new[] { "a", "b" });
            var l2 = new List<string>(new[] { "a", "b" });
            // Pre-Assert
            // Act
            Expect(() =>
            {
                PropertyAssert.AreDeepEqual(l1, l2);
            }, Throws.Nothing);
            // Assert
        }

        public class BaseClass
        {
            public virtual string Name { get; set; }
        }
        public class DerivedClass: BaseClass
        {
            public override string Name => "Some constant";
        }

        private void DoTest(BaseClass b1, BaseClass b2)
        {
            PropertyAssert.AreDeepEqual(b1, b2);
        }

        [Test]
        public void AreEqual_IsNotConfusedByOverridingProperties()
        {
            //---------------Set up test pack-------------------
            var d1 = new DerivedClass();
            var d2 = new BaseClass { Name = "Some constant" };
            
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
                d1.prop,
                ignoreMe = d1.ignoreMe + 1
            };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => d1.DeepEquals(d2, "ignoreMe"));

            //---------------Test Result -----------------------
        }

        [Test]
        public void IntersectionEquals_WildFailure()
        {
            //--------------- Arrange -------------------
            var ducked = Create.InstanceOf<ITraveller>();
            var traveller = GetRandom<Traveller>();
            traveller.CopyPropertiesTo(ducked);

            //--------------- Assume ----------------

            //--------------- Act ----------------------

            //--------------- Assert -----------------------
            PropertyAssert.AreIntersectionEqual(ducked, traveller);
        }

        public class HasEnumerableStuff
        {
            public IEnumerable<string> Stuff { get; set ; }
        }

        public class HasCollectionOfStuff
        {
            public ICollection<string> Stuff { get; set; }
        }

        [Test]
        public void IntersectionEquals_ShouldBeAbleToCompare_ICollection_And_IEnumerable()
        {
            //--------------- Arrange -------------------
            var dataSource = new[] { "1", "a", "%" };
            var collection = new Collection<string>(dataSource);
            var enumerable = dataSource.AsEnumerable();
            var left = new HasCollectionOfStuff { Stuff = collection };
            var right = new HasEnumerableStuff { Stuff = enumerable };

            Assert.IsTrue(left.Stuff.GetType().ImplementsEnumerableGenericType());
            Assert.IsTrue(right.Stuff.GetType().ImplementsEnumerableGenericType());

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            PropertyAssert.AreIntersectionEqual(left, right);

            //--------------- Assert -----------------------
        }

        public class IntersectionEqualsTestCandidate1
        {
            public int NoIgnoringMe { get; set; }
            public string NonMatching { get; set ; }
            public bool IgnoreMe { get; set; }
        }
        public class IntersectionEqualsTestCandidate2
        {
            public int NoIgnoringMe { get; set; }
            public string AlsoNonMatching { get; set ; }
            public bool IgnoreMe { get; set; }
        }

        [Test]
        public void IntersectionEquals_ShouldIgnoreSpecifiedProperties()
        {
            //--------------- Arrange -------------------
            var left = GetRandom<IntersectionEqualsTestCandidate1>();
            var right = GetRandom<IntersectionEqualsTestCandidate2>();
            left.NoIgnoringMe = right.NoIgnoringMe;
            left.IgnoreMe = !right.IgnoreMe;

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            PropertyAssert.AreIntersectionEqual(left, right, "IgnoreMe");

            //--------------- Assert -----------------------
        }



        public class Traveller: ITraveller
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string IdNumber { get; set; }
            public string[] PassportNumbers { get; set; }
            public string MealPreferences { get; set; }
            public string TravelPreferences { get; set; }
        }
        public interface IActor
        {
            Guid Id { get; set; }
            string Name { get; set; }
            string Email { get; set; }
        }
        public interface ITravellerDetails
        {
            string IdNumber { get; set; }
            string[] PassportNumbers { get; set; }
            string MealPreferences { get; set; } // Halaal? Vegan?
            string TravelPreferences { get; set; } // seats near emergency exits for more leg-room?
        }
        public interface ITraveller : IActor, ITravellerDetails
        {
        }
    }

}
