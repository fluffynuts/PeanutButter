using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.Utils;

namespace PenautButter.Utils.Tests
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
                RandomValueGen.GetRandomInt(),
                RandomValueGen.GetRandomInt(),
                RandomValueGen.GetRandomInt()
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
            var delimiter = RandomValueGen.GetRandomString(2, 3);
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
        public void Randomize_ShouldReturnTheSameCollectionInADifferentOrder()
        {
            for (var i = 0; i < 10; i++)    // it's entirely possible that the random output is the same, so let's try up to 10 times
            {
                //---------------Set up test pack-------------------
                var src = RandomValueGen.GetRandomList(() => RandomValueGen.GetRandomString(), 3);

                //---------------Assert Precondition----------------
                CollectionAssert.IsNotEmpty(src);

                //---------------Execute Test ----------------------
                var result = src.Randomize();

                //---------------Test Result -----------------------
                CollectionAssert.AreEquivalent(result, src);
                try
                {
                    CollectionAssert.AreNotEqual(result, src);
                }
                catch (Exception)
                {
                    if (i < 9)
                        continue;
                    throw;
                }
            }
        }
    }
}
