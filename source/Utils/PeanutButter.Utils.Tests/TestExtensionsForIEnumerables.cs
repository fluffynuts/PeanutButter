using System;
using System.Collections.Generic;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestExtensionsForIEnumerables
    {
        [Test]
        public void ForEach_OperatingOnNullCollection_ShouldThrowArgumentNullException()
        {
            // This may seem pointless, but bear with me:
            //  I actually wanted ForEach to just do nothing when given a null collection,
            //  however System.Collections.Generic.List<T> has its own ForEach signature
            //  which doesn't even throw a valid ArgumentNullException -- it just barfs
            //  with a NullReferenceException. So if I made mine different, it would be
            //  inconsistent and surprising. And extension methods can't override actual
            //  methods. So, poo. This test then serves as documentation that I thought
            //  about it and chose consistency over function. You can, however, use
            //  the helper EmptyIfNull() to get the nicer behaviour:
            //  int[] foo = null;
            //  foo.EmptyIfNull().ForEach(i => {});
            //---------------Set up test pack-------------------
            int[] src = null;

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<NullReferenceException>(() => src.ForEach(i => { }));

            //---------------Test Result -----------------------
        }

        [Test]
        public void ForEach_OperatingOnCollection_ShouldRunActionOnEachElement()
        {
            //---------------Set up test pack-------------------
            var result = new List<int>();
            var src = new[]
            {
                GetRandomInt(),
                GetRandomInt(),
                GetRandomInt()
            };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            src.ForEach(item => result.Add(item));

            //---------------Test Result -----------------------
            CollectionAssert.AreEqual(src, result);
        }

        [Test]
        public void IsSameAs_OperatingOnCollection_WhenBothAreNull_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            List<int> first = null;
            List<int> second = null;

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = first.IsSameAs(second);

            //---------------Test Result -----------------------
            Assert.IsTrue(result);
        }

        [Test]
        public void IsSameAs_OperatingOnCollection_WhenOneIsNull_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            List<int> first = null;
            var second = new List<int>();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = first.IsSameAs(second);

            //---------------Test Result -----------------------
            Assert.IsFalse(result);
        }

        [Test]
        public void IsSameAs_OperatingOnCollection_WhenBothAreEmpty_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            var first = new int[] {};
            var second = new List<int>();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = first.IsSameAs(second);

            //---------------Test Result -----------------------
            Assert.IsTrue(result);
        }

        [Test]
        public void IsSameAs_OperatingOnCollection_WhenBothContainSameElements_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            var first = new int[] { 1 };
            var second = new List<int>(new[] { 1 });

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = first.IsSameAs(second);

            //---------------Test Result -----------------------
            Assert.IsTrue(result);
        }

        [Test]
        public void IsSameAs_OperatingOnCollection_WhenBothContainSameElementsInDifferentOrder_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            var first = new int[] { 1, 2 };
            var second = new List<int>(new[] { 2, 1 });

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = first.IsSameAs(second);

            //---------------Test Result -----------------------
            Assert.IsTrue(result);
        }

        [Test]
        public void IsSameAs_OperatingOnCollection_WhenBothContainDifferentElements_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            var first = new int[] { 1 };
            var second = new List<int>(new[] { 2, 1 });

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = first.IsSameAs(second);

            //---------------Test Result -----------------------
            Assert.IsFalse(result);
        }

        [Test]
        public void JoinWith_OperatingOnCollection_WhenCollectionIsEmpty_ShouldReturnEmptyString()
        {
            //---------------Set up test pack-------------------
            var src = new List<string>();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = src.JoinWith(",");

            //---------------Test Result -----------------------
            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void JoinWith_OperatingOnCollection_WhenCollectionIsNotEmpty_ShouldReturnCollectionJoinedWithGivenDelimiter()
        {
            //---------------Set up test pack-------------------
            var src = new int[] {1, 2, 3};
            var delimiter = GetRandomString(2, 3);
            var expected = "1" + delimiter + "2" + delimiter + "3";

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = src.JoinWith(delimiter);

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void IsEmpty_WhenCollectionIsEmpty_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            var src = new Stack<int>();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = src.IsEmpty();

            //---------------Test Result -----------------------
            Assert.IsTrue(result);
        }

        [Test]
        public void IsEmpty_WhenCollectionIsNotEmpty_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            var src = new[] {1};

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = src.IsEmpty();

            //---------------Test Result -----------------------
            Assert.IsFalse(result);
        }

        [Test]
        public void EmptyIfNull()
        {
            //---------------Set up test pack-------------------
            List<int> src = null;
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = src.EmptyIfNull();

            //---------------Test Result -----------------------
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<List<int>>(result);
            CollectionAssert.IsEmpty(result);
        }

        [Test]
        public void And_OperatingOnArrayOfType_ShouldReturnNewArrayWithAddedItems()
        {
            //---------------Set up test pack-------------------
            var src = new[] {1, 2, 3};

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = src.And(4, 5);

            //---------------Test Result -----------------------
            CollectionAssert.AreEqual(new[] {1, 2, 3, 4, 5}, result);
        }

        [Test]
        public void ButNot_OperatingOnArrayOfType_ShouldReturnNewArrayWithAddedItems()
        {
            //---------------Set up test pack-------------------
            var src = new[] {1, 2, 3};

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = src.ButNot(2);

            //---------------Test Result -----------------------
            CollectionAssert.AreEqual(new[] {1, 3 }, result);
        }

        [Test]
        public void Flatten_GivenEmptyCollection_ShouldReturnEmptyCollection()
        {
            //---------------Set up test pack-------------------
            var input = new List<List<int>>();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.Flatten();

            //---------------Test Result -----------------------
            Assert.IsNotNull(result);
            CollectionAssert.IsEmpty(result);
        }

        [Test]
        public void Flatten_GivenCollectionWithOneItemInSubCollection_ShouldReturnFlattened()
        {
            //---------------Set up test pack-------------------
            var input = new List<IEnumerable<int>>();
            var expected = GetRandomCollection<int>(1,1);
            input.Add(expected);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.Flatten();

            //---------------Test Result -----------------------
            Assert.IsNotNull(result);
            CollectionAssert.IsNotEmpty(result);
            CollectionAssert.AreEqual(expected, result);
        }

        [Test]
        public void Flatten_GivenCollectionWithMultipleItemsInMultipleSubCollections_ShouldReturnFlattened()
        {
            //---------------Set up test pack-------------------
            var input = new List<IEnumerable<int>>();
            var part1 = GetRandomCollection<int>();
            var part2 = GetRandomCollection<int>();
            var part3 = GetRandomCollection<int>();
            var expected = new List<int>();
            expected.AddRange(part1);
            expected.AddRange(part2);
            expected.AddRange(part3);
            input.AddRange(new[] { part1, part2, part3 }); 

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.Flatten();

            //---------------Test Result -----------------------
            CollectionAssert.AreEquivalent(expected, result);
        }

        private class ItemWithNullableId
        {
            public int? Id { get; set; }
            public static ItemWithNullableId For(int? value)
            {
                return new ItemWithNullableId() { Id = value };
            }
        }

        [Test]
        public void SelectNonNull_GivenCollectionOfObjectsWithNullableInts_ShouldReturnOnlyNonNullValues()
        {
            //---------------Set up test pack-------------------
            var id1 = GetRandomInt();
            var id2 = GetRandomInt();
            var expected = new[] { id1, id2 };
            var input = new[] 
            {
                ItemWithNullableId.For(id1),
                ItemWithNullableId.For(null),
                ItemWithNullableId.For(id2),
                ItemWithNullableId.For(null)
            };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.SelectNonNull(o => o.Id);

            //---------------Test Result -----------------------
            CollectionAssert.AreEqual(expected, result);
        }


        public class Thing
        {
        }
        public class ItemWithNullableThing
        {
            public Thing Thing { get; set; }
            public static ItemWithNullableThing For(Thing thing)
            {
                return new ItemWithNullableThing() { Thing = thing };
            }
        }

        [Test]
        public void SelectNonNull_GivenCollectionOfObjectsWithNullableThings_ShouldReturnOnlyNonNullValues()
        {
            //---------------Set up test pack-------------------
            var id1 = GetRandom<Thing>();
            var id2 = GetRandom<Thing>();
            var expected = new[] { id1, id2 };
            var input = new[] 
            {
                ItemWithNullableThing.For(id1),
                ItemWithNullableThing.For(null),
                ItemWithNullableThing.For(id2),
                ItemWithNullableThing.For(null)
            };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.SelectNonNull(o => o.Thing);

            //---------------Test Result -----------------------
            CollectionAssert.AreEqual(expected, result);
        }

        [Test]
        public void AsText_OperatingOnStringArray_ShouldReturnTextBlockWithEnvironmentNewlines()
        {
            //---------------Set up test pack-------------------
            var input = GetRandomCollection<string>(2,4);
            var expected = string.Join(Environment.NewLine, input);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.AsText();

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }

        public class SomethingWithNiceToString
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public override string ToString()
            {
                return $"{Id} :: {Name}";
            }
        }

        [Test]
        public void AsText_OperatingOnArrayOfObjects_ShouldReturnTextBlockWithStringRepresentations()
        {
            //---------------Set up test pack-------------------
            var input = GetRandomCollection<SomethingWithNiceToString>(2,4);
            var expected = string.Join(Environment.NewLine, input);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.AsText();

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void AsText_GivenAlternativeDelimiter_ShouldUseIt()
        {
            //---------------Set up test pack-------------------
            var input = GetRandomCollection<int>(3,6);
            var delimiter = GetRandomString(1);
            var expected = string.Join(delimiter, input);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.AsText(delimiter);

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void HasUnique_WhenNoMatchesForLambda_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            var input = new[] { 1,2,3 };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.IsFalse(input.HasUnique(i => i == 4));

            //---------------Test Result -----------------------
        }

        [Test]
        public void HasUnique_WhenMultipleMatchesForLambda_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            var input = new[] { "a", "a", "b", "c" };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.IsFalse(input.HasUnique(i => i == "a"));

            //---------------Test Result -----------------------
        }

        [Test]
        public void HasUnique_WhenOneMatchesForLambda_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            var input = new[] { "a", "a", "b", "c" };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.IsTrue(input.HasUnique(i => i == "b"));

            //---------------Test Result -----------------------
        }




    }
}
