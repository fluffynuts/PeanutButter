using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using PeanutButter.DuckTyping.Extensions;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestObjectExtensions: AssertionHelper
    {
        [Test]
        public void DeepEquals_GivenSourceWithNoPropsAndDestWithNoProps_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = (new object()).DeepEquals(new object());

            //---------------Test Result -----------------------
            Assert.IsTrue(result);
        }


        [Test]
        public void DeepEquals_GivenTwoObjectsBothWithTheSamePropertyNameAndValue_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var randomString = RandomValueGen.GetRandomString();
            var result = (new { prop = randomString }).DeepEquals(new { prop = randomString });

            //---------------Test Result -----------------------
            Assert.IsTrue(result);
        }

        [Test]
        public void DeepSubEquals_WhenDestinationHasMorePropertiesButSameNamedOnesMatch_ReturnsTrue()
        {
            //---------------Set up test pack-------------------
            var rs = RandomValueGen.GetRandomString();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = (new { prop = rs }).DeepSubEquals(new { prop = rs, bar = RandomValueGen.GetRandomString() });

            //---------------Test Result -----------------------
            Assert.IsTrue(result);
        }

        [Test]
        public void DeepSubEquals_WhenDestinationIsMissingProperty_ReturnsFalse()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = (new { prop = RandomValueGen.GetRandomString() })
                .DeepSubEquals(new object());

            //---------------Test Result -----------------------
            Assert.IsFalse(result);
        }

        [Test]
        public void DeepEquals_WhenStringPropertyDoesntMatch_ReturnsFalse()
        {
            //---------------Set up test pack-------------------
            var propVal = RandomValueGen.GetRandomString();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = (new { prop = propVal })
                .DeepEquals(new { prop = propVal + RandomValueGen.GetRandomString(1, 10) });

            //---------------Test Result -----------------------
            Assert.IsFalse(result);
        }

        [Test]
        public void DeepEquals_DoesntBarfOnBothNull()
        {
            //---------------Set up test pack-------------------
            object o1 = null;
            object o2 = null;
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = o1.DeepEquals(o2);

            //---------------Test Result -----------------------
            Assert.IsTrue(result);
        }

        [Test]
        public void DeepEquals_DoesntBarfOnSourceNull()
        {
            //---------------Set up test pack-------------------
            object o1 = null;
            object o2 = new object();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = o1.DeepEquals(o2);

            //---------------Test Result -----------------------
            Assert.IsFalse(result);
        }

        [Test]
        public void DeepEquals_DoesntBarfOnTargetNull()
        {
            //---------------Set up test pack-------------------
            object o2 = null;
            object o1 = new object();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = o1.DeepEquals(o2);

            //---------------Test Result -----------------------
            Assert.IsFalse(result);
        }

        [Test]
        public void DeepEquals_WhenComparingIdenticalPropertiesOfSimpleTypes_ShouldReturnTrue()
        {
            TestPropertyMatchingFor<int>();
            TestPropertyMatchingFor<long>();
            TestPropertyMatchingFor<byte>();
            TestPropertyMatchingFor<char>();
            TestPropertyMatchingFor<float>();
            TestPropertyMatchingFor<double>();
            TestPropertyMatchingFor<decimal>();
            TestPropertyMatchingFor<string>();
            TestPropertyMatchingFor<DateTime>();
            TestPropertyMatchingFor<int?>();
            TestPropertyMatchingFor<long?>();
            TestPropertyMatchingFor<byte?>();
            TestPropertyMatchingFor<char?>();
            TestPropertyMatchingFor<float?>();
            TestPropertyMatchingFor<double?>();
            TestPropertyMatchingFor<decimal?>();
            TestPropertyMatchingFor<DateTime?>();
        }

        private static void TestPropertyMatchingFor<T>()
        {
            //---------------Set up test pack-------------------
            var propVal = (T)RandomValueGen.GetRandom<T>();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = (new { prop = propVal }).DeepEquals(new { prop = propVal });

            //---------------Test Result -----------------------
            Assert.IsTrue(result);
        }

        [Test]
        public void DeepEquals_ComplexTypesAreTraversed_HappyCase()
        {
            //---------------Set up test pack-------------------
            var propVal = RandomValueGen.GetRandomString();
            var o1 = new {
                prop = new {
                    bar = propVal
                }
            };
            var o2 = new {
                prop = new {
                    bar = propVal
                }
            };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = o1.DeepEquals(o2);

            //---------------Test Result -----------------------
            Assert.IsTrue(result);
        }

        [Test]
        public void DeepEquals_ComplexTypesAreTraversed_UnhappyCase()
        {
            //---------------Set up test pack-------------------
            var o1 = new {
                prop = new {
                    bar = RandomValueGen.GetRandomString(1, 10)
                }
            };
            var o2 = new {
                prop = new {
                    bar = RandomValueGen.GetRandomString(11, 20)
                }
            };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = o1.DeepEquals(o2);

            //---------------Test Result -----------------------
            Assert.IsFalse(result);
        }

        [Test]
        public void DeepEquals_WhenGivenOnePropertiesToIgnoreByName_ShouldIgnoreThosePropertiesInTheComparison()
        {
            //---------------Set up test pack-------------------
            var o1 = new {
                testMe = "foo",
                ignoreMe = 1
            };
            var o2 = new {
                testMe = o1.testMe,
                ignoreMe = o1.ignoreMe + 1
            };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = o1.DeepEquals(o2, "ignoreMe");

            //---------------Test Result -----------------------
            Assert.IsTrue(result);
        }

        [Test]
        public void DeepEquals_WhenGivenListOfPropertiesToIgnoreByName_ShouldIgnoreThosePropertiesInTheComparison()
        {
            //---------------Set up test pack-------------------
            var o1 = new {
                testMe = "foo",
                ignoreMe1 = 1,
                ignoreMe2 = 2
            };
            var o2 = new {
                testMe = o1.testMe,
                ignoreMe1 = o1.ignoreMe1 + 1,
                ignoreMe2 = o1.ignoreMe1
            };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = o1.DeepEquals(o2, "ignoreMe1", "ignoreMe2");

            //---------------Test Result -----------------------
            Assert.IsTrue(result);
        }


        [Test]
        public void DeepEquals_WhenGivenArrayOfPropertiesToIgnoreByName_ShouldIgnoreThosePropertiesInTheComparison()
        {
            //---------------Set up test pack-------------------
            var o1 = new {
                testMe = "foo",
                ignoreMe1 = 1,
                ignoreMe2 = 2
            };
            var o2 = new {
                testMe = o1.testMe,
                ignoreMe1 = o1.ignoreMe1 + 1,
                ignoreMe2 = o1.ignoreMe1
            };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = o1.DeepEquals(o2, new[] { "ignoreMe1", "ignoreMe2" });

            //---------------Test Result -----------------------
            Assert.IsTrue(result);
        }

        [Test]
        public void DeepSubEquals_GivenSourceAndDestWithNoOverlappingProps_ShouldReturnFalse()
        {
            //--------------- Arrange -------------------
            var left = new {
                foo = "bar"
            };
            var right = new {
                quuz = "wibbles"
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = left.DeepSubEquals(right);

            //--------------- Assert -----------------------
            Expect(result, Is.False);
        }

        [Test]
        public void DeepIntersectionEquals_GivenSourceAndDestWithMatchingOverlappingProps_ShouldReturnTrue()
        {
            //--------------- Arrange -------------------
            var left = new {
                foo = "bar",
                name = "Mickey"
            };
            var right = new {
                quuz = "wibbles",
                name = "Mickey"
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = left.DeepIntersectionEquals(right);

            //--------------- Assert -----------------------
            Expect(result, Is.True);
        }

        [Test]
        public void DeepIntersectionEquals_GivenSourceAndDestWithNoOverlappingProps_ShouldReturnFalse()
        {
            //--------------- Arrange -------------------
            var left = new {
                foo = "bar",
            };
            var right = new {
                quuz = "wibbles",
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = left.DeepIntersectionEquals(right);

            //--------------- Assert -----------------------
            Expect(result, Is.False);
        }


        [Test]
        public void CopyPropertiesTo_GivenSimpleObjectDest_DoesNotThrow()
        {
            //---------------Set up test pack-------------------
            var src = new {
                prop = RandomValueGen.GetRandomString()
            };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => src.CopyPropertiesTo(new object()));

            //---------------Test Result -----------------------
        }

        public class Simple<T>
        {
            public T prop { get; set; }
            public Simple()
            {
                Randomize();
            }

            public void Randomize()
            {
                prop = (T)RandomValueGen.GetRandom<T>();
            }
        }

        [Test]
        public void CopyPropertiesTo_GivenDestWithSameProperty_CopiesValue()
        {
            TestCopyFor<string>();
            TestCopyFor<int>();
            TestCopyFor<byte>();
            TestCopyFor<char>();
            TestCopyFor<long>();
            TestCopyFor<float>();
            TestCopyFor<double>();
            TestCopyFor<decimal>();
            TestCopyFor<DateTime>();
            TestCopyFor<bool>();
            TestCopyFor<int?>();
            TestCopyFor<byte?>();
            TestCopyFor<char?>();
            TestCopyFor<long?>();
            TestCopyFor<float?>();
            TestCopyFor<double?>();
            TestCopyFor<decimal?>();
            TestCopyFor<DateTime?>();
            TestCopyFor<bool?>();
        }

        private static void TestCopyFor<T>()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var src = new Simple<T>();
            var dst = new Simple<T>();
            while (dst.prop.Equals(src.prop))
                dst.Randomize();
            src.CopyPropertiesTo(dst);

            //---------------Test Result -----------------------
            Assert.AreEqual(src.prop, dst.prop);
        }

        public class Complex<T>
        {
            public Simple<T> prop { get; set; }
            public Complex()
            {
                prop = new Simple<T>();
            }
        }

        [Test]
        public void CopyPropertiesTo_ComplexTypesAreTraversedButOnlySimplePropertiesAreCopied()
        {
            //---------------Set up test pack-------------------
            var src = new Complex<int>();
            var dst = new Complex<int>();
            while (dst.prop.prop.Equals(src.prop.prop))
                dst.prop.Randomize();

            //---------------Assert Precondition----------------
            Assert.AreNotEqual(src, dst);
            Assert.AreNotEqual(src.prop, dst.prop);
            Assert.AreNotEqual(src.prop.prop, dst.prop.prop);
            //---------------Execute Test ----------------------
            src.CopyPropertiesTo(dst);

            //---------------Test Result -----------------------
            Assert.AreNotEqual(src.prop, dst.prop);
            Assert.AreEqual(src.prop.prop, dst.prop.prop);
        }
        [Test]
        public void CopyPropertiesTo_WhenDeepIsFalse_ComplexTypesAreTraversedAndRefCopied()
        {
            //---------------Set up test pack-------------------
            var src = new Complex<int>();
            var dst = new Complex<int>();
            while (dst.prop.prop.Equals(src.prop.prop))
                dst.prop.Randomize();

            //---------------Assert Precondition----------------
            Assert.AreNotEqual(src, dst);
            Assert.AreNotEqual(src.prop, dst.prop);
            Assert.AreNotEqual(src.prop.prop, dst.prop.prop);
            //---------------Execute Test ----------------------
            src.CopyPropertiesTo(dst, false);

            //---------------Test Result -----------------------
            Assert.AreEqual(src.prop, dst.prop);
            Assert.AreEqual(src.prop.prop, dst.prop.prop);
        }


        [Test]
        public void CopyPropertiesTo_DoesntBarfOnANullTargetThatIsComplex()
        {
            //---------------Set up test pack-------------------
            var o1 = new Complex<string>();
            var o2 = new Complex<string>();
            o2.prop = null;

            //---------------Assert Precondition----------------
            Assert.IsNull(o2.prop);

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => o1.CopyPropertiesTo(o2));

            //---------------Test Result -----------------------
        }
        [Test]
        public void CopyPropertiesTo_DoesntBarfOnANullSourceThatIsComplex()
        {
            //---------------Set up test pack-------------------
            var o1 = new Complex<string>();
            var o2 = new Complex<string>();
            o1.prop = null;

            //---------------Assert Precondition----------------
            Assert.IsNull(o1.prop);

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => o1.CopyPropertiesTo(o2));

            //---------------Test Result -----------------------
            Assert.IsNull(o2.prop);
        }

        [Test]
        public void GetOrDefault_WhenGivenNameOfPropertyWhichDoesNotExist_ShouldReturnDefaultValue()
        {
            //---------------Set up test pack-------------------
            var o = new { };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = o.GetOrDefault("prop", 1);

            //---------------Test Result -----------------------
            Assert.AreEqual(1, result);
        }

        [Test]
        public void Get_WhenGivenNameOfPropertyWhichDoesNotExist_ShouldThrow_PropertyNotFoundException()
        {
            //---------------Set up test pack-------------------
            var o = new { };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<PropertyNotFoundException>(() => o.Get<bool>("prop"));

            //---------------Test Result -----------------------
        }


        [Test]
        public void Get_WhenGivenNameOfPropertyWhichDoesExist_ShouldReturnThatValue()
        {
            //---------------Set up test pack-------------------
            var o = new { prop = 2 };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = o.GetOrDefault("prop", 1);

            //---------------Test Result -----------------------
            Assert.AreEqual(2, result);
        }

        [Test]
        public void Get_WhenGivenNamefPropertyWhichDoesExistAndIncorrectType_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var o = new { prop = 2 };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<ArgumentException>(() => o.GetOrDefault<string>("prop"));

            //---------------Test Result -----------------------
        }

        [Test]
        public void Get_ShouldBeAbleToResolveADotTree()
        {
            //---------------Set up test pack-------------------
            var parent = new {
                child = new {
                    prop = 2
                }
            };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = parent.GetOrDefault<int>("child.prop");

            //---------------Test Result -----------------------
            Assert.AreEqual(2, result);
        }

        [Test]
        public void GetPropertyValue_ShouldReturnValueOfNamedProperty()
        {
            //---------------Set up test pack-------------------
            var obj = new { id = RandomValueGen.GetRandomInt() };
            var expected = obj.id;

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = obj.GetPropertyValue("id");

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }

        public class SomeSimpleType
        {
            public int Id { get; set; }
        }

        [Test]
        public void SetPropertyValue_ShouldSetThePropertyValue()
        {
            //---------------Set up test pack-------------------
            var obj = new SomeSimpleType() { Id = RandomValueGen.GetRandomInt(2, 5) };
            var expected = RandomValueGen.GetRandomInt(10, 20);
            const string propertyName = "Id";

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            obj.SetPropertyValue(propertyName, expected);
            var result = obj.GetPropertyValue(propertyName);

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void AsArray_ShouldWrapObjectInArray()
        {
            //---------------Set up test pack-------------------
            var sut = new object();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = sut.AsArray();

            //---------------Test Result -----------------------
            Assert.IsNotNull(result);
            CollectionAssert.IsNotEmpty(result);
            Assert.AreEqual(sut, result.Single());
        }

        public class SimpleDto
        {
            public string Name { get; set; }
        }

        public class ParentDto
        {
            public SimpleDto Child { get; set; }
        }

        public class GrandParentDto
        {
            public ParentDto Parent { get; set; }
        }

        [Test]
        public void SetPropertyValue_ShouldBeAbleToSetImmediateProperty()
        {
            //---------------Set up test pack-------------------
            var obj = new SimpleDto() { Name = GetRandomString() };
            var expected = GetAnother(obj.Name);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            obj.SetPropertyValue("Name", expected);

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, obj.Name);
        }

        [Test]
        public void SetPropertyValue_ShouldBeAbleToSetDottedProperty()
        {
            //---------------Set up test pack-------------------
            var obj = new ParentDto()
            {
                Child = new SimpleDto()
                {
                    Name = GetRandomString()
                }
            };
            var expected = GetRandomString();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            obj.SetPropertyValue("Child.Name", expected);

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, obj.Child.Name);
        }


        [Test]
        public void SetPropertyValue_ShouldBeAbleToSetDottedPropertyFurtherDOwn()
        {
            //---------------Set up test pack-------------------
            var obj = new GrandParentDto()
            {
                Parent = new ParentDto()
                {
                    Child = new SimpleDto()
                    {
                        Name = GetRandomString()
                    }
                }
            };
            var expected = GetRandomString();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            obj.SetPropertyValue("Parent.Child.Name", expected);

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, obj.Parent.Child.Name);
        }

        public class ThingWithCollection<T>
        {
            public IEnumerable<T> Collection { get; set; }
        }

        [Test]
        public void AllPropertiesMatch_WhenCollectionsMatch_ShouldReturnTrue()
        {
            //--------------- Arrange -------------------
            var item = new ThingWithCollection<int>()
            {
                Collection = new int[] { 1, 2 }
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = item.DeepEquals(item);

            //--------------- Assert -----------------------
            Assert.IsTrue(result);
        }

        [Test]
        public void AllPropertiesMatch_WhenCollectionsMisMatchByCount_ShouldReturnFalse()
        {
            //--------------- Arrange -------------------
            var item1 = new ThingWithCollection<int>()
            {
                Collection = new int[] { 1, 2 }
            };
            var item2 = new ThingWithCollection<int>()
            {
                Collection = new int[] { 1, 2, 3 }
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = item1.DeepEquals(item2);

            //--------------- Assert -----------------------
            Assert.IsFalse(result);
        }

        [Test]
        public void AllPropertiesMatch_WhenCollectionsMisMatchByValue_ShouldReturnFalse()
        {
            //--------------- Arrange -------------------
            var item1 = new ThingWithCollection<int>()
            {
                Collection = new int[] { 1, 2 }
            };
            var item2 = new ThingWithCollection<int>()
            {
                Collection = new int[] { 1, 1 }
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = item1.DeepEquals(item2);

            //--------------- Assert -----------------------
            Assert.IsFalse(result);
        }

        [Test]
        public void DeepEquals_ShouldNotStackOverflowLikeInTheWild()
        {
            //--------------- Arrange -------------------
            var item = GetRandom<ITravelRequest>();
            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Assert.DoesNotThrow(() => item.DeepEquals(item));

            //--------------- Assert -----------------------
        }

        [TestCase(true)]
        [TestCase(false)]
        [TestCase("foo")]
        [TestCase(123)]
        [TestCase(1.23)]
        public void DeepEquals_GivenTwoEqualPrimitiveTypes_ShouldReturnTrue_(object value)
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = value.DeepEquals(value);

            //--------------- Assert -----------------------
            Assert.IsTrue(result);
        }


        private class SomeBaseClass
        {
            public int Id { get; set; }
        }

        private class SomeDerivedClass : SomeBaseClass
        {
            public string Name { get; set; }
        }

        [Test]
        public void DeepEquals_InheritedPropertiesShouldMatter()
        {
            //--------------- Arrange -------------------
            var item1 = new SomeDerivedClass() { Id = 1, Name = "Bob" };
            var item2 = new SomeDerivedClass() { Id = 2, Name = "Bob" };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = item1.DeepEquals(item2);

            //--------------- Assert -----------------------
            Assert.IsFalse(result);
        }

        public class Parent
        {
            public IEnumerable<Child> Children { get; set; }
        }

        public class Child
        {
            public Parent Parent { get; set; }
        }

        [Test]
        public void DeepEquals_ShouldNotStackOverflowWithCircularReferences_Level1()
        {
            //--------------- Arrange -------------------
            var parent = new Parent();
            var child = new Child() { Parent = parent };
            parent.Children = new[] { child };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Assert.DoesNotThrow(() => parent.DeepEquals(parent));

            //--------------- Assert -----------------------
        }

        public class Node
        {
            public IEnumerable<Node> Children { get; set; }
        }

        [Test]
        public void DeepEquals_ShouldNotStackOverflowWithCircularReferences_Level2()
        {
            //--------------- Arrange -------------------
            var n2 = new Node();
            var n1 = new Node() { Children = new[] { n2 } };
            var n3 = new Node() { Children = new[] { n1 } };
            n2.Children = new[] { n3 };

            // n1 => n2 =>  n3 => n1 ....

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Assert.DoesNotThrow(() => n1.DeepEquals(n1));

            //--------------- Assert -----------------------
        }

        [Test]
        public void DeepEquals_ShouldNotStackOverflowWithCircularReferences_Level2_v2()
        {
            //--------------- Arrange -------------------
            var n2 = new Node();
            var n1 = new Node() { Children = new[] { n2 } };
            var n3 = new Node() { Children = new[] { n1 } };
            n2.Children = new[] { n3 };

            // n1 => n2 =>  n3 => n1 ....

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Assert.DoesNotThrow(() => n1.DeepEquals(n2));

            //--------------- Assert -----------------------
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

        public class ActualTravellerDetails: ITravellerDetails
        {
            public string IdNumber { get; set; }
            public string[] PassportNumbers { get; set; }
            public string MealPreferences { get; set; }
            public string TravelPreferences { get; set; }
        }

        [Test]
        public void CopyPropertiesTo_ShouldCopyProperties()
        {
            //--------------- Arrange -------------------
            var actor = GetRandom<IActor>().DuckAs<IActor>();
            var details = GetRandom<ActualTravellerDetails>();
            var traveller = GetRandom<Traveller>(); 

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            actor.CopyPropertiesTo(traveller);
            details.CopyPropertiesTo(traveller);

            //--------------- Assert -----------------------
            PropertyAssert.IntersectionEquals(actor, traveller);
            PropertyAssert.IntersectionEquals(details, traveller);
        }


        public class HasAnArray
        {
            public string[] Stuff { get; set; }
        }
        [Test]
        public void CopyPropertiesTo_ShouldCopyEmptyArrayProperty()
        {
            //--------------- Arrange -------------------
            var src = new HasAnArray() { Stuff = new string[0] };
            var target = new HasAnArray();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            src.CopyPropertiesTo(target);

            //--------------- Assert -----------------------
            Assert.IsNotNull(target.Stuff);
            CollectionAssert.IsEmpty(target.Stuff);
        }

        [Test]
        public void CopyPropertiesTo_ShouldCopyNonEmptyArrayProperty()
        {
            //--------------- Arrange -------------------
            var src = new HasAnArray() { Stuff = new [] { "123", "456" } };
            var target = new HasAnArray();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            src.CopyPropertiesTo(target);

            //--------------- Assert -----------------------
            Assert.IsNotNull(target.Stuff);
            CollectionAssert.IsNotEmpty(target.Stuff);
            CollectionAssert.AreEqual(src.Stuff, target.Stuff);
        }

        public class HasAnEnumerable
        {
            public IEnumerable<string> Stuff { get; set; }
        }
        [Test]
        public void CopyPropertiesTo_ShouldCopyNonEmptyEnumerableProperty()
        {
            //--------------- Arrange -------------------
            var src = new HasAnEnumerable() { Stuff = new [] { "123", "456" } };
            var target = new HasAnEnumerable();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            src.CopyPropertiesTo(target);

            //--------------- Assert -----------------------
            Assert.IsNotNull(target.Stuff);
            CollectionAssert.IsNotEmpty(target.Stuff);
            CollectionAssert.AreEqual(src.Stuff, target.Stuff);
        }

        public class HasIdAndName
        {
            public int Id { get; set ; }
            public string Name { get; set; }
        }
        [Test]
        public void CopyPropertiesTo_ShouldAllowIgnoreList()
        {
            //--------------- Arrange -------------------
            var src = new HasIdAndName() {
                Id = 1,
                Name = GetRandomString()
            };
            var target = new HasIdAndName() {
                Id = 2,
                Name = GetRandomString()
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            src.CopyPropertiesTo(target, "Id");

            //--------------- Assert -----------------------
            Expect(target.Name, Is.EqualTo(src.Name));
            Expect(target.Id, Is.EqualTo(2));
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


        public enum TravelRequestStatuses
        {
            Created,
            QuotesRequired,
            ApprovalRequired,
            Rejected,
            Approved,
            Cancelled
        }

        public interface ITravelRequestDetails
        {
            DateTime Initiated { get; set; }
            string DepartingFrom { get; set; }
            string TravellingTo { get; set; }
            DateTime ExpectedDeparture { get; set; }
            string PreferredDepartureTime { get; set; }
            DateTime ExpectedReturn { get; set; }
            string PreferredReturnTime { get; set; }
            string Reason { get; set; }
            bool CarRequired { get; set; }
            bool AccomodationRequired { get; set; }
            string AccommodationRequiredNotes { get; set; }
        }
        public interface ITravelQuote
        {
            Guid TravelRequestId { get; set; }
            Guid AddedById { get; set; }
            Guid QuoteId { get; set; }
            decimal Cost { get; set; }
            // try to lower the input requirements for the user at this point
            //  - ideally that person can copy/paste from an email
            string Details { get; set; }
            bool AcceptedByTraveller { get; set; }
        }
        public interface IComment
        {
            Guid CommenterId { get; set; }
            string CommentText { get; set; }
            DateTime CommentedAt { get; set; }
        }

        public interface ITravelRequest
        {
            Guid Id { get; set; }
            IActor CurrentlyAssignedTo { get; set; }
            ITraveller Traveller { get; set; }
            ITravelRequestDetails Details { get; set; }

            TravelRequestStatuses RequestStatus { get; set; }
            IEnumerable<ITravelQuote> Quotes { get; set; }
            IEnumerable<IComment> Comments { get; set; }
        }
        public class TravelRequestDetails : ITravelRequestDetails
        {
            public DateTime Initiated { get; set; }
            public string DepartingFrom { get; set; }
            public string TravellingTo { get; set; }
            public DateTime ExpectedDeparture { get; set; }
            public string PreferredDepartureTime { get; set; }
            public DateTime ExpectedReturn { get; set; }
            public string PreferredReturnTime { get; set; }
            public string Reason { get; set; }
            public bool CarRequired { get; set; }
            public bool AccomodationRequired { get; set; }
            public string AccommodationRequiredNotes { get; set; }
        }

        public class TravelRequest : ITravelRequest
        {
            public Guid Id { get; set; }
            public IActor CurrentlyAssignedTo { get; set; }
            public ITraveller Traveller { get; set; }
            public ITravelRequestDetails Details { get; set; }
            public TravelRequestStatuses RequestStatus { get; set; }
            public IEnumerable<ITravelQuote> Quotes { get; set; }
            public IEnumerable<IComment> Comments { get; set; }

            public TravelRequest()
            {
                // drop this when this becomes an actual db entity
                Quotes = new List<ITravelQuote>();
                Comments = new List<IComment>();
                Details = new TravelRequestDetails();
            }
        }

    }
}
