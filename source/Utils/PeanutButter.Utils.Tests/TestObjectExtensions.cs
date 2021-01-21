using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Serialization;
using NUnit.Framework;
using PeanutButter.DuckTyping.Extensions;
using PeanutButter.TestUtils.Generic;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using static NExpect.Expectations;

// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable PossibleNullReferenceException
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable ExpressionIsAlwaysNull
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming
// ReSharper disable MemberHidesStaticFromOuterClass
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedMember.Global

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestObjectExtensions
    {
        [TestFixture]
        public class DeepEqualityTesting
        {
            [Test]
            public void GivenSourceWithNoPropsAndDestWithNoProps_ShouldReturnTrue()
            {
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = (new object()).DeepEquals(new object());

                //---------------Test Result -----------------------
                Assert.IsTrue(result);
            }


            [Test]
            public void GivenTwoObjectsBothWithTheSamePropertyNameAndValue_ShouldReturnTrue()
            {
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var randomString = GetRandomString();
                var result = (new {prop = randomString}).DeepEquals(new {prop = randomString});

                //---------------Test Result -----------------------
                Assert.IsTrue(result);
            }

            [Test]
            public void WhenStringPropertyDoesntMatch_ReturnsFalse()
            {
                //---------------Set up test pack-------------------
                var propVal = GetRandomString();
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = (new {prop = propVal})
                    .DeepEquals(new {prop = propVal + GetRandomString(1, 10)});

                //---------------Test Result -----------------------
                Assert.IsFalse(result);
            }

            [Test]
            public void DoesntBarfOnBothNull()
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
            public void DoesntBarfOnSourceNull()
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
            public void DoesntBarfOnTargetNull()
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
            public void WhenComparingIdenticalPropertiesOfSimpleTypes_ShouldReturnTrue()
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
                var propVal = GetRandom<T>();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = (new {prop = propVal}).DeepEquals(new {prop = propVal});

                //---------------Test Result -----------------------
                Assert.IsTrue(result);
            }

            [Test]
            public void ComplexTypesAreTraversed_HappyCase()
            {
                //---------------Set up test pack-------------------
                var propVal = GetRandomString();
                var o1 = new
                {
                    prop = new
                    {
                        bar = propVal
                    }
                };
                var o2 = new
                {
                    prop = new
                    {
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
            public void ComplexTypesAreTraversed_UnhappyCase()
            {
                //---------------Set up test pack-------------------
                var o1 = new
                {
                    prop = new
                    {
                        bar = GetRandomString(1, 10)
                    }
                };
                var o2 = new
                {
                    prop = new
                    {
                        bar = GetRandomString(11, 20)
                    }
                };

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = o1.DeepEquals(o2);

                //---------------Test Result -----------------------
                Assert.IsFalse(result);
            }

            [Test]
            public void WhenGivenOnePropertiesToIgnoreByName_ShouldIgnoreThosePropertiesInTheComparison()
            {
                //---------------Set up test pack-------------------
                var o1 = new
                {
                    testMe = "foo",
                    ignoreMe = 1
                };
                var o2 = new
                {
                    o1.testMe,
                    ignoreMe = o1.ignoreMe + 1
                };

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = o1.DeepEquals(o2, "ignoreMe");

                //---------------Test Result -----------------------
                Assert.IsTrue(result);
            }

            [Test]
            public void WhenGivenListOfPropertiesToIgnoreByName_ShouldIgnoreThosePropertiesInTheComparison()
            {
                //---------------Set up test pack-------------------
                var o1 = new
                {
                    testMe = "foo",
                    ignoreMe1 = 1,
                    ignoreMe2 = 2
                };
                var o2 = new
                {
                    o1.testMe,
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
            public void WhenGivenArrayOfPropertiesToIgnoreByName_ShouldIgnoreThosePropertiesInTheComparison()
            {
                //---------------Set up test pack-------------------
                var o1 = new
                {
                    testMe = "foo",
                    ignoreMe1 = 1,
                    ignoreMe2 = 2
                };
                var o2 = new
                {
                    o1.testMe,
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
            public void WhenObjectsHaveDifferentCollections_ShouldReturnFalse()
            {
                // Arrange
                var val = GetRandomString();
                var first = new HasAnArrayOfStrings() {Strings = new[] {val}};
                var second = new HasAnArrayOfStrings() {Strings = new string[] { }};
                // Pre-Assert
                // Act
                var result1 = first.DeepEquals(second);
                var result2 = second.DeepEquals(first);
                // Assert
                Expect(result1).To.Be.False();
                Expect(result2).To.Be.False();
            }

            [Test]
            public void WhenObjectsHaveSameCollections_ShouldReturnTrue()
            {
                // Arrange
                var val = GetRandomString();
                var first = new HasAnArrayOfStrings() {Strings = new[] {val}};
                var second = new HasAnArrayOfStrings() {Strings = new[] {val}};
                // Pre-Assert
                // Act
                var result1 = first.DeepEquals(second);
                var result2 = second.DeepEquals(first);
                // Assert
                Expect(result1).To.Be.True();
                Expect(result2).To.Be.True();
            }

            public class HasSomethingWithStrings
            {
                public HasAnArrayOfStrings Something { get; set; }
            }

            [Test]
            public void WhenSubObjectsHaveDifferentCollections_ShouldReturnFalse()
            {
                // Arrange
                var val = GetRandomString();
                var first = new HasSomethingWithStrings()
                {
                    Something = new HasAnArrayOfStrings() {Strings = new[] {val}}
                };
                var second = new HasSomethingWithStrings()
                {
                    Something = new HasAnArrayOfStrings() {Strings = new string[] { }}
                };
                // Pre-Assert
                // Act
                var result1 = first.DeepEquals(second);
                var result2 = second.DeepEquals(first);
                // Assert
                Expect(result1).To.Be.False();
                Expect(result2).To.Be.False();
            }

            [Test]
            public void WhenSubObjectsHaveSameCollections_ShouldReturnTrue()
            {
                // Arrange
                var val = GetRandomString();
                var first = new HasSomethingWithStrings()
                {
                    Something = new HasAnArrayOfStrings() {Strings = new[] {val}}
                };
                var second = new HasSomethingWithStrings()
                {
                    Something = new HasAnArrayOfStrings() {Strings = new[] {val}}
                };
                // Pre-Assert
                // Act
                var result1 = first.DeepEquals(second);
                var result2 = second.DeepEquals(first);
                // Assert
                Expect(result1).To.Be.True();
                Expect(result2).To.Be.True();
            }

            [Test]
            public void WhenCollectionsMatch_ShouldReturnTrue()
            {
                //--------------- Arrange -------------------
                var item = new ThingWithCollection<int>()
                {
                    Collection = new[] {1, 2}
                };

                //--------------- Assume ----------------

                //--------------- Act ----------------------
                var result = item.DeepEquals(item);

                //--------------- Assert -----------------------
                Assert.IsTrue(result);
            }

            [Test]
            public void WhenCollectionsMisMatchByCount_ShouldReturnFalse()
            {
                //--------------- Arrange -------------------
                var item1 = new ThingWithCollection<int>()
                {
                    Collection = new[] {1, 2}
                };
                var item2 = new ThingWithCollection<int>()
                {
                    Collection = new[] {1, 2, 3}
                };

                //--------------- Assume ----------------

                //--------------- Act ----------------------
                var result = item1.DeepEquals(item2);

                //--------------- Assert -----------------------
                Assert.IsFalse(result);
            }

            [Test]
            public void WhenCollectionsMisMatchByValue_ShouldReturnFalse()
            {
                //--------------- Arrange -------------------
                var item1 = new ThingWithCollection<int>()
                {
                    Collection = new[] {1, 2}
                };
                var item2 = new ThingWithCollection<int>()
                {
                    Collection = new[] {1, 1}
                };

                //--------------- Assume ----------------

                //--------------- Act ----------------------
                var result = item1.DeepEquals(item2);

                //--------------- Assert -----------------------
                Assert.IsFalse(result);
            }

            [Test]
            public void ShouldNotStackOverflowLikeInTheWild()
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
            public void GivenTwoEqualPrimitiveTypes_ShouldReturnTrue_(object value)
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
            public void InheritedPropertiesShouldMatter()
            {
                //--------------- Arrange -------------------
                var item1 = new SomeDerivedClass()
                {
                    Id = 1,
                    Name = "Bob"
                };
                var item2 = new SomeDerivedClass()
                {
                    Id = 2,
                    Name = "Bob"
                };

                //--------------- Assume ----------------

                //--------------- Act ----------------------
                var result = item1.DeepEquals(item2);

                //--------------- Assert -----------------------
                Assert.IsFalse(result);
            }

            public class SimpleParent
            {
                public IEnumerable<Child> Children { get; set; }
            }

            public class Child
            {
                public SimpleParent Parent { get; set; }
            }

            [Test]
            public void ShouldNotStackOverflowWithCircularReferences_Level1()
            {
                //--------------- Arrange -------------------
                var parent = new SimpleParent();
                var child = new Child() {Parent = parent};
                parent.Children = new[] {child};

                //--------------- Assume ----------------

                //--------------- Act ----------------------
                Assert.DoesNotThrow(() => parent.DeepEquals(parent));

                //--------------- Assert -----------------------
            }

            public class NodeWithChildren
            {
                public IEnumerable<NodeWithChildren> Children { get; set; }
            }

            [Test]
            public void ShouldNotStackOverflowWithCircularReferences_Level2()
            {
                //--------------- Arrange -------------------
                var n2 = new NodeWithChildren();
                var n1 = new NodeWithChildren() {Children = new[] {n2}};
                var n3 = new NodeWithChildren() {Children = new[] {n1}};
                n2.Children = new[] {n3};

                // n1 => n2 =>  n3 => n1 ....

                //--------------- Assume ----------------

                //--------------- Act ----------------------
                Assert.DoesNotThrow(() => n1.DeepEquals(n1));

                //--------------- Assert -----------------------
            }

            [Test]
            public void ShouldNotStackOverflowWithCircularReferences_Level2_v2()
            {
                //--------------- Arrange -------------------
                var n2 = new NodeWithChildren();
                var n1 = new NodeWithChildren() {Children = new[] {n2}};
                var n3 = new NodeWithChildren() {Children = new[] {n1}};
                n2.Children = new[] {n3};

                // n1 => n2 =>  n3 => n1 ....

                //--------------- Assume ----------------

                //--------------- Act ----------------------
                Assert.DoesNotThrow(() => n1.DeepEquals(n2));

                //--------------- Assert -----------------------
            }

            [TestFixture]
            public class OperatingOnCollections
            {
                [Test]
                public void ContainsOneDeepEqualTo_WhenCollectionItemWithSameData_ShouldReturnTrue()
                {
                    //--------------- Arrange -------------------
                    var collection1 = new[]
                    {
                        new {id = 1}
                    };

                    //--------------- Assume ----------------

                    //--------------- Act ----------------------
                    var result = collection1.ContainsAtLeastOneDeepEqualTo(new {id = 1});

                    //--------------- Assert -----------------------
                    Expect(result).To.Be.True();
                }

                [Test]
                public void ContainsOneDeepEqualTo_WhenCollectionItemWithDifferentData_ShouldReturnFalse()
                {
                    //--------------- Arrange -------------------
                    var collection1 = new[]
                    {
                        new {id = 1}
                    };

                    //--------------- Assume ----------------

                    //--------------- Act ----------------------
                    var result = collection1.ContainsAtLeastOneDeepEqualTo(new {id = 2});

                    //--------------- Assert -----------------------
                    Expect(result).To.Be.False();
                }

                [Test]
                public void ContainsOneDeepEqualTo_WhenCollectionItemWithMismatchedProperties_ShouldReturnFalse()
                {
                    //--------------- Arrange -------------------
                    var collection1 = new[]
                    {
                        new
                        {
                            id = 1,
                            name = "Bob"
                        }
                    };

                    //--------------- Assume ----------------

                    //--------------- Act ----------------------
                    var result = collection1.ContainsAtLeastOneDeepEqualTo(new {id = 1});

                    //--------------- Assert -----------------------
                    Expect(result).To.Be.False();
                }

                [Test]
                public void ContainsOneDeepEqualTo_WhenCollectionItemWithMismatchedIgnoredProperties_ShouldReturnTrue()
                {
                    //--------------- Arrange -------------------
                    var collection1 = new[]
                    {
                        new
                        {
                            id = 1,
                            name = "Bob"
                        }
                    };

                    //--------------- Assume ----------------

                    //--------------- Act ----------------------
                    var result = collection1.ContainsAtLeastOneDeepEqualTo(new {id = 1}, "name");

                    //--------------- Assert -----------------------
                    Expect(result).To.Be.True();
                }

                [Test]
                public void ContainsOnlyOneDeepEqualTo_WhenCollectionContainsNoMatches_ShouldReturnFalse()
                {
                    //--------------- Arrange -------------------
                    var collection1 = new[]
                    {
                        new
                        {
                            id = 1,
                            name = "Bob"
                        }
                    };

                    //--------------- Assume ----------------

                    //--------------- Act ----------------------
                    var result = collection1.ContainsOnlyOneDeepEqualTo(
                        new
                        {
                            id = 2,
                            name = "Bob"
                        });

                    //--------------- Assert -----------------------
                    Expect(result).To.Be.False();
                }

                [Test]
                public void ContainsOnlyOneDeepEqualTo_WhenCollectionContainsOneMatch_ShouldReturnTrue()
                {
                    //--------------- Arrange -------------------
                    var collection1 = new[]
                    {
                        new
                        {
                            id = 1,
                            name = "Bob"
                        }
                    };

                    //--------------- Assume ----------------

                    //--------------- Act ----------------------
                    var result = collection1.ContainsOnlyOneDeepEqualTo(
                        new
                        {
                            id = 1,
                            name = "Bob"
                        });

                    //--------------- Assert -----------------------
                    Expect(result).To.Be.True();
                }

                [Test]
                public void
                    ContainsOnlyOneDeepEqualTo_WhenCollectionContainsOneMatchWithIgnoredProperty_ShouldReturnTrue()
                {
                    //--------------- Arrange -------------------
                    var collection1 = new[]
                    {
                        new
                        {
                            id = 1,
                            name = "Bob",
                            ignore = "moo"
                        }
                    };

                    //--------------- Assume ----------------

                    //--------------- Act ----------------------
                    var result = collection1.ContainsOnlyOneDeepEqualTo(
                        new
                        {
                            id = 1,
                            name = "Bob"
                        },
                        "ignore");

                    //--------------- Assert -----------------------
                    Expect(result).To.Be.True();
                }

                [Test]
                public void ContainsOnlyOneDeepEqualTo_WhenCollectionContainsTwoMatches_ShouldReturnFalse()
                {
                    //--------------- Arrange -------------------
                    var collection1 = new[]
                    {
                        new
                        {
                            id = 1,
                            name = "Bob"
                        },
                        new
                        {
                            id = 1,
                            name = "Bob"
                        }
                    };

                    //--------------- Assume ----------------

                    //--------------- Act ----------------------
                    var result = collection1.ContainsOnlyOneDeepEqualTo(
                        new
                        {
                            id = 1,
                            name = "Bob"
                        });

                    //--------------- Assert -----------------------
                    Expect(result).To.Be.False();
                }


                [Test]
                public void ContainsOnlyOneIntersectionEqualTo_WhenCollectionContainsNoMatches_ShouldReturnFalse()
                {
                    //--------------- Arrange -------------------
                    var collection1 = new[]
                    {
                        new
                        {
                            id = 1,
                            name = "Bob"
                        }
                    };

                    //--------------- Assume ----------------

                    //--------------- Act ----------------------
                    var result = collection1.ContainsOnlyOneIntersectionEqualTo(
                        new
                        {
                            id = 2,
                            name = "Bob"
                        });

                    //--------------- Assert -----------------------
                    Expect(result).To.Be.False();
                }

                [Test]
                public void ContainsOnlyOneIntersectionEqualTo_WhenCollectionContainsOneMatch_ShouldReturnTrue()
                {
                    //--------------- Arrange -------------------
                    var collection1 = new[]
                    {
                        new
                        {
                            id = 1,
                            name = "Bob"
                        }
                    };

                    //--------------- Assume ----------------

                    //--------------- Act ----------------------
                    var result = collection1.ContainsOnlyOneIntersectionEqualTo(
                        new
                        {
                            id = 1,
                            name = "Bob"
                        });

                    //--------------- Assert -----------------------
                    Expect(result).To.Be.True();
                }

                [Test]
                public void
                    ContainsOnlyOneIntersectionEqualTo_WhenCollectionContainsOneMatchWithIgnoredProperty_ShouldReturnTrue()
                {
                    //--------------- Arrange -------------------
                    var collection1 = new[]
                    {
                        new
                        {
                            id = 1,
                            name = "Bob",
                            moo = "cow"
                        }
                    };

                    //--------------- Assume ----------------

                    //--------------- Act ----------------------
                    var result = collection1.ContainsOnlyOneIntersectionEqualTo(
                        new
                        {
                            id = 1,
                            name = "Bob"
                        },
                        "moo");

                    //--------------- Assert -----------------------
                    Expect(result).To.Be.True();
                }

                [Test]
                public void ContainsOnlyOneIntersectionEqualTo_WhenCollectionContainsTwoMatches_ShouldReturnFalse()
                {
                    //--------------- Arrange -------------------
                    var collection1 = new[]
                    {
                        new
                        {
                            id = 1,
                            name = "Bob"
                        },
                        new
                        {
                            id = 1,
                            name = "Bob"
                        }
                    };

                    //--------------- Assume ----------------

                    //--------------- Act ----------------------
                    var result = collection1.ContainsOnlyOneIntersectionEqualTo(
                        new
                        {
                            id = 1,
                            name = "Bob"
                        });

                    //--------------- Assert -----------------------
                    Expect(result).To.Be.False();
                }
            }
        }

        [Test]
        public void CopyPropertiesTo_GivenSimpleObjectDest_DoesNotThrow()
        {
            //---------------Set up test pack-------------------
            var src = new
            {
                prop = GetRandomString()
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
                prop = GetRandom<T>();
            }
        }

        [TestFixture]
        public class CopyingPropertiesFromOneObjectToAnother
        {
            [Test]
            public void GivenDestWithSameProperty_CopiesValue()
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


            [Test]
            public void ComplexTypesAreTraversedButOnlySimplePropertiesAreCopied()
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
            public void WhenDeepIsFalse_ComplexTypesAreTraversedAndRefCopied()
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
            public void DoesntBarfOnANullTargetThatIsComplex()
            {
                //---------------Set up test pack-------------------
                var o1 = new Complex<string>();
                var o2 = new Complex<string> {prop = null};

                //---------------Assert Precondition----------------
                Assert.IsNull(o2.prop);

                //---------------Execute Test ----------------------
                Assert.DoesNotThrow(() => o1.CopyPropertiesTo(o2));

                //---------------Test Result -----------------------
            }

            [Test]
            public void DoesntBarfOnANullSourceThatIsComplex()
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
            public void ShouldCopyProperties()
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
                PropertyAssert.AreIntersectionEqual(actor, traveller);
                PropertyAssert.AreIntersectionEqual(details, traveller);
            }

            [Test]
            public void ShouldCopyEmptyArrayProperty()
            {
                //--------------- Arrange -------------------
                var src = new HasAnArrayOfStrings() {Strings = new string[0]};
                var target = new HasAnArrayOfStrings();

                //--------------- Assume ----------------

                //--------------- Act ----------------------
                src.CopyPropertiesTo(target);

                //--------------- Assert -----------------------
                Assert.IsNotNull(target.Strings);
                CollectionAssert.IsEmpty(target.Strings);
            }

            [Test]
            public void ShouldCopyNonEmptyArrayProperty()
            {
                //--------------- Arrange -------------------
                var src = new HasAnArrayOfStrings() {Strings = new[] {"123", "456"}};
                var target = new HasAnArrayOfStrings();

                //--------------- Assume ----------------

                //--------------- Act ----------------------
                src.CopyPropertiesTo(target);

                //--------------- Assert -----------------------
                Assert.IsNotNull(target.Strings);
                CollectionAssert.IsNotEmpty(target.Strings);
                CollectionAssert.AreEqual(src.Strings, target.Strings);
            }

            [Test]
            public void ShouldCopyNonEmptyEnumerableProperty()
            {
                //--------------- Arrange -------------------
                var src = new HasAnEnumerable() {Stuff = new[] {"123", "456"}};
                var target = new HasAnEnumerable();

                //--------------- Assume ----------------

                //--------------- Act ----------------------
                src.CopyPropertiesTo(target);

                //--------------- Assert -----------------------
                Assert.IsNotNull(target.Stuff);
                CollectionAssert.IsNotEmpty(target.Stuff);
                CollectionAssert.AreEqual(src.Stuff, target.Stuff);
            }

            [Test]
            public void ShouldCopyAnEmptyList()
            {
                // Arrange
                var src = new HasAListOfStrings() {Strings = new List<string>()};
                var target = new HasAListOfStrings();
                // Pre-Assert
                Expect(target.Strings).To.Be.Null();
                // Act
                src.CopyPropertiesTo(target);
                // Assert
                Expect(target.Strings).Not.To.Be.Null();
                Expect(target.Strings).To.Be.Empty();
            }

            [Test]
            public void ShouldCopyAnNonEmptyList()
            {
                // Arrange
                var src = new HasAListOfDates()
                {
                    DateTimes = GetRandomCollection<DateTime>(2, 3).ToList()
                };
                var target = new HasAListOfDates();
                // Pre-Assert
                Expect(target.DateTimes).To.Be.Null();
                // Act
                src.CopyPropertiesTo(target);
                // Assert
                Expect(target.DateTimes).Not.To.Be.Null();
                Expect(target.DateTimes).Not.To.Be.Empty();
                src.DateTimes.ShouldMatchDataIn(target.DateTimes);
            }

            [Test]
            public void ShouldAllowIgnoreList()
            {
                //--------------- Arrange -------------------
                var src = new HasIdAndName()
                {
                    Id = 1,
                    Name = GetRandomString()
                };
                var target = new HasIdAndName()
                {
                    Id = 2,
                    Name = GetRandomString()
                };

                //--------------- Assume ----------------

                //--------------- Act ----------------------
                src.CopyPropertiesTo(target, "Id");

                //--------------- Assert -----------------------
                Expect(target.Name).To.Equal(src.Name);
                Expect(target.Id).To.Equal(2);
            }
        }

        [TestFixture]
        public class AccessingPropertiesByName
        {
            [TestFixture]
            public class GetOrDefault
            {
                [Test]
                public void WhenGivenNameOfPropertyWhichDoesNotExist_ShouldReturnDefaultValue()
                {
                    //---------------Set up test pack-------------------
                    var o = new { };

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    var result = o.GetOrDefault("prop", 1);

                    //---------------Test Result -----------------------
                    Assert.AreEqual(1, result);
                }
            }

            [TestFixture]
            public class Get
            {
                [Test]
                public void WhenGivenNameOfPropertyWhichDoesNotExist_ShouldThrow_PropertyNotFoundException()
                {
                    //---------------Set up test pack-------------------
                    var o = new { };

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    Assert.Throws<MemberNotFoundException>(() => o.Get<bool>("prop"));

                    //---------------Test Result -----------------------
                }


                [Test]
                public void WhenGivenNameOfPropertyWhichDoesExist_ShouldReturnThatValue()
                {
                    //---------------Set up test pack-------------------
                    var o = new {prop = 2};

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    var result = o.GetOrDefault("prop", 1);

                    //---------------Test Result -----------------------
                    Assert.AreEqual(2, result);
                }

                [Test]
                public void WhenGivenNamefPropertyWhichDoesExistAndIncorrectType_ShouldThrow()
                {
                    //---------------Set up test pack-------------------
                    var o = new {prop = 2};

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    Assert.Throws<ArgumentException>(() => o.GetOrDefault<string>("prop"));

                    //---------------Test Result -----------------------
                }

                public interface IPerson
                {
                    int Id { get; set; }
                    string Name { get; set; }
                }

                public class Person : IPerson
                {
                    public int Id { get; set; }
                    public string Name { get; set; }
                }

                [Test]
                public void ShouldBeAbleToRetrieveByInterface()
                {
                    // Arrange
                    var person = GetRandom<Person>();
                    var o = new {Person = person};

                    Expect(typeof(Person).IsAssignableTo<IPerson>())
                        .To.Be.True();
                    // Pre-assert
                    // Act
                    var result = o.Get<IPerson>("Person");
                    // Assert
                    Expect(result).To.Be(person);
                }

                [Test]
                public void ShouldBeAbleToResolveADotTree()
                {
                    //---------------Set up test pack-------------------
                    var parent = new
                    {
                        child = new
                        {
                            prop = 2
                        }
                    };

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    var result = parent.GetOrDefault<int>("child.prop");

                    //---------------Test Result -----------------------
                    Assert.AreEqual(2, result);
                }
            }

            [TestFixture]
            public class GetAndSetPropertyValue
            {
                [Test]
                public void ShouldReturnValueOfNamedProperty()
                {
                    //---------------Set up test pack-------------------
                    var obj = new {id = GetRandomInt()};
                    var expected = obj.id;

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    var result = obj.GetPropertyValue("id");

                    //---------------Test Result -----------------------
                    Assert.AreEqual(expected, result);
                }

                [Test]
                public void ShouldTraverseDottedNames()
                {
                    // Arrange
                    var expected = GetRandomString();
                    var input = new
                    {
                        Child = new
                        {
                            Name = expected
                        }
                    };
                    // Pre-Assert
                    // Act
                    var result = input.GetPropertyValue("Child.Name");
                    // Assert
                    Expect(result).To.Equal(expected);
                }

                public class SomeSimpleType
                {
                    public int Id { get; set; }
                }

                [Test]
                public void ShouldSetThePropertyValue()
                {
                    //---------------Set up test pack-------------------
                    var obj = new SomeSimpleType() {Id = GetRandomInt(2, 5)};
                    var expected = GetRandomInt(10, 20);
                    const string propertyName = "Id";

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    obj.SetPropertyValue(propertyName, expected);
                    var result = obj.GetPropertyValue(propertyName);

                    //---------------Test Result -----------------------
                    Assert.AreEqual(expected, result);
                }

                [Test]
                public void ShouldSetThePropertyValueGeneric()
                {
                    //---------------Set up test pack-------------------
                    var obj = new SomeSimpleType() {Id = GetRandomInt(2, 5)};
                    var expected = GetRandomInt(10, 20);
                    const string propertyName = "Id";

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    obj.Set(propertyName, expected);
                    var result = obj.Get<int>(propertyName);

                    //---------------Test Result -----------------------
                    Assert.AreEqual(expected, result);
                }

                [Test]
                public void ShouldBeAbleToSetImmediateProperty()
                {
                    //---------------Set up test pack-------------------
                    var obj = new SimpleDto() {Name = GetRandomString()};
                    var expected = GetAnother(obj.Name);

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    obj.SetPropertyValue("Name", expected);

                    //---------------Test Result -----------------------
                    Assert.AreEqual(expected, obj.Name);
                }

                [Test]
                public void ShouldBeAbleToSetDottedProperty()
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
                public void ShouldBeAbleToSetDottedPropertyFurtherDOwn()
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
            }
        }

        [TestFixture]
        public class AccessingFieldsByPath
        {
            [Test]
            public void ShouldGetAndSetPublicField()
            {
                // Arrange
                var poco = new Poco();
                var expected1 = GetRandomInt(1);
                var expected2 = GetAnother(expected1);
                // Act
                poco.Set(nameof(Poco.PublicField), expected2);
                var result = poco.Get<int>(nameof(Poco.PublicField));
                // Assert
                Expect(result)
                    .To.Equal(expected2);
            }
            
            [Test]
            public void ShouldGetAndSetPrivateField()
            {
                // Arrange
                var poco = new Poco();
                var expected1 = GetRandomInt(1);
                var expected2 = GetAnother(expected1);
                // Act
                poco.Set("_privateField", expected2);
                var result = poco.Get<int>("_privateField");
                // Assert
                Expect(result)
                    .To.Equal(expected2);
            }

#pragma warning disable 169
            public class Poco
            {
                public int PublicField;
                private int _privateField;
            }
#pragma warning restore 169
        }

        [TestFixture]
        public class DeepClone
        {
            public class EmptyType
            {
            }

            [Test]
            public void GivenEmptyObject_ShouldReturnNewEmptyObject()
            {
                // Arrange
                var src = new EmptyType();
                // Pre-Assert
                // Act
                var result = src.DeepClone();
                // Assert
                Expect(result).Not.To.Be.Null();
                Expect(result).To.Be.An.Instance.Of<EmptyType>();
                Expect(result.GetType().GetProperties()).To.Be.Empty();
            }

            public class Node
            {
                public int Id { get; set; }
                public string Name { get; set; }
                public Guid Guid { get; set; }
                public bool Flag { get; set; }
            }

            [Test]
            public void ShouldCloneFirstLevel()
            {
                // Arrange
                var src = GetRandom<Node>();
                // Pre-Assert
                // Act
                var result = src.DeepClone();
                // Assert
                PropertyAssert.AreDeepEqual(src, result);
            }


            public class Parent : Node
            {
                public Node Child { get; set; }
            }

            [Test]
            public void ShouldCloneSecondLevelButNotChildRefs()
            {
                // Arrange
                var src = GetRandom<Parent>();
                // Pre-Assert
                Expect(src.Child).Not.To.Be.Null();
                // Act
                var result = src.DeepClone();
                // Assert
                Expect(result.Child).Not.To.Equal(src.Child);
                Expect(result).To.Deep.Equal(src);
            }

            public enum Emotions
            {
                Unknown = 0,
                Happy = 1,
                Sad = 2
            }

            public class Puppy
            {
                public Emotions Emotion { get; set; }
            }

            [Test]
            public void ShouldCopyEnumProperties()
            {
                // Arrange
                var src = new Puppy() {Emotion = Emotions.Happy};

                // Pre-Assert

                // Act
                var result = src.DeepClone();

                // Assert
                Expect(result.Emotion).To.Equal(Emotions.Happy);
            }


            public class HasAnArray
            {
                public Node[] Nodes { get; set; }
            }

            [Test]
            public void ShouldCloneAnArrayProperty()
            {
                // Arrange
                var src = GetRandom<HasAnArray>();
                src.Nodes = GetRandomCollection<Node>(2, 4).ToArray();
                // Pre-Assert
                // Act
                var result = src.DeepClone();
                // Assert
                Expect(result.Nodes).Not.To.Be.Empty();
                result.Nodes.ShouldMatchDataIn(src.Nodes);
            }

            public class HasAnIEnumerable
            {
                public IEnumerable<Node> Nodes { get; set; }
            }

            [Test]
            public void ShouldCloneAnIEnumerableProperty()
            {
                // Arrange
                var src = GetRandom<HasAnIEnumerable>();
                src.Nodes = GetRandomCollection<Node>(2, 4).ToArray();
                // Pre-Assert
                // Act
                var result = src.DeepClone();
                // Assert
                Expect(result.Nodes).Not.To.Be.Empty();
                result.Nodes.ShouldMatchDataIn(src.Nodes);
            }

            public class HasAList
            {
                public List<Node> Nodes { get; set; }
            }

            [Test]
            public void ShouldCloneAGenericListProperty()
            {
                // Arrange
                var src = GetRandom<HasAList>();
                src.Nodes = GetRandomCollection<Node>(2, 4).ToList();
                // Pre-Assert
                // Act
                var result = src.DeepClone();
                // Assert
                Expect(result.Nodes).Not.To.Be.Empty();
                result.Nodes.ShouldMatchDataIn(src.Nodes);
            }

            public class HasAName
            {
                public string Name { get; set; }
            }

            public class HasANameAndLabel : HasAName
            {
                public string Label { get; set; }
            }

            [Test]
            public void ShouldCloneTheActualTypeAndCast()
            {
                // Arrange
                var src = GetRandom<HasANameAndLabel>();
                var downCast = (HasAName) src;
                // Pre-Assert
                // Act
                var result = downCast.DeepClone();
                // Assert
                var upcast = result as HasANameAndLabel;
                Expect(upcast).Not.To.Be.Null();
                Expect(upcast.Label).To.Equal(src.Label);
            }

            [Test]
            public void ShouldReturnNullFromOriginalNull()
            {
                // Arrange
                HasANameAndLabel src = null;
                // Pre-Assert
                // Act
                var result = src.DeepClone();
                // Assert
                Expect(result).To.Be.Null();
            }

            [Test]
            public void ShouldCloneANullGenericListProperty()
            {
                // Arrange
                var src = GetRandom<HasAList>();
                src.Nodes = null;
                // Pre-Assert
                // Act
                var result = src.DeepClone();
                // Assert
                Expect(result.Nodes).To.Be.Null();
            }

            [Test]
            public void ShouldCloneAStandaloneArray()
            {
                // Arrange
                var src = GetRandomArray<Node>(2);
                // Act
                var result = src.DeepClone();
                // Assert
                Expect(result).To.Deep.Equal(src);
            }

            [Test]
            public void ShouldCloneAStandaloneList()
            {
                // Arrange
                var src = GetRandomArray<Node>(2).ToList();
                // Act
                var result = src.DeepClone();
                // Assert
                Expect(result).To.Deep.Equal(src);
            }

            [Test]
            public void ShouldCloneAStandaloneIList()
            {
                // Arrange
                IList<Node> src = GetRandomArray<Node>(2).ToList();
                // Act
                var result = src.DeepClone();
                // Assert
                Expect(result).To.Deep.Equal(src);
            }

            [Test]
            public void ShouldCloneCustomImplementationOfDictionary()
            {
                // Arrange
                var src = new HasCustomDictionary
                {
                    Settings = new SerializableDictionary<string, object> {["moo"] = "cow"}
                };
                // Pre-Assert
                // Act
                var result = src.DeepClone();
                // Assert
                Expect(result).Not.To.Be.Null();
                Expect(result.Settings).Not.To.Be.Null();
                Expect(result.Settings as IDictionary<string, object>).To.Contain.Key("moo")
                    .With.Value("cow");
            }

            [Test]
            public void ShouldCloneRegularOldVanillaDictionary()
            {
                // Arrange
                var src = new HasVanillaDictionary
                {
                    Settings = new SerializableDictionary<string, object> {["moo"] = "cow"}
                };
                // Pre-Assert
                // Act
                var result = src.DeepClone();
                // Assert
                Expect(result).Not.To.Be.Null();
                Expect(result.Settings).Not.To.Be.Null();
                Expect(result.Settings as IDictionary<string, object>).To.Contain.Key("moo")
                    .With.Value("cow");
            }

            public class HasVanillaDictionary
            {
                public Dictionary<string, object> Settings { get; set; }
            }

            public class HasCustomDictionary
            {
                public SerializableDictionary<string, object> Settings { get; set; }
            }

            [XmlRoot("dictionary")]
            public class SerializableDictionary<TKey, TValue>
                : Dictionary<TKey, TValue>, IXmlSerializable
            {
                public System.Xml.Schema.XmlSchema GetSchema()
                {
                    return null;
                }


                public void ReadXml(System.Xml.XmlReader reader)
                {
                    XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
                    XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

                    bool wasEmpty = reader.IsEmptyElement;
                    reader.Read();
                    if (wasEmpty)
                        return;


                    while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
                    {
                        reader.ReadStartElement("item");

                        reader.ReadStartElement("key");
                        TKey key = (TKey) keySerializer.Deserialize(reader);
                        reader.ReadEndElement();

                        reader.ReadStartElement("value");
                        TValue value = (TValue) valueSerializer.Deserialize(reader);
                        reader.ReadEndElement();

                        Add(key, value);

                        reader.ReadEndElement();
                        reader.MoveToContent();
                    }

                    reader.ReadEndElement();
                }


                public void WriteXml(System.Xml.XmlWriter writer)
                {
                    XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
                    XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

                    foreach (TKey key in Keys)
                    {
                        writer.WriteStartElement("item");

                        writer.WriteStartElement("key");
                        keySerializer.Serialize(writer, key);
                        writer.WriteEndElement();

                        writer.WriteStartElement("value");
                        TValue value = this[key];
                        valueSerializer.Serialize(writer, value);
                        writer.WriteEndElement();

                        writer.WriteEndElement();
                    }
                }
            }


            public static IEnumerable<Node> CollectionOfNodes()
            {
                yield return new Node()
                {
                    Flag = true,
                    Guid = Guid.Parse("CBCEAB18-6A92-48D3-A699-998D7BB8DA0E"),
                    Id = 1,
                    Name = "one"
                };
                yield return new Node()
                {
                    Flag = false,
                    Guid = Guid.Parse("23B63474-911E-409A-9C23-D8A2A8324FAA"),
                    Id = 2,
                    Name = "two"
                };
            }

            [Test]
            public void ShouldCloneAStandaloneIEnumerable()
            {
                // Arrange
                IEnumerable<Node> src = CollectionOfNodes();
                // Act
                Console.WriteLine(src.GetType());
                var result = src.DeepClone();
                // Assert
                Expect(result).To.Deep.Equal(src);
            }

            [Test]
            public void ShouldCloneAStandaloneDictionary()
            {
                // Arrange
                var src = new Dictionary<string, string>()
                {
                    ["moo"] = "cow"
                };
                // Pre-Assert
                // Act
                var result = src.DeepClone();
                // Assert
                Expect(result).Not.To.Be.Null();
                Expect(result)
                    .To.Contain.Key("moo")
                    .With.Value("cow");
            }

            public class HasADictionary
            {
                public Dictionary<string, int> Prop { get; set; }
            }

            [Test]
            public void ShouldCloneADictionaryProperty()
            {
                // Arrange
                var src = new HasADictionary()
                {
                    Prop = new Dictionary<string, int>()
                    {
                        ["one"] = 1,
                        ["two"] = 2
                    }
                };
                // Pre-Assert
                // Act
                var result = src.DeepClone();
                // Assert
                Expect(result).Not.To.Be.Null();
                Expect(result.Prop).Not.To.Be.Null();
                Expect(result.Prop).To.Be.Deep.Equivalent.To(src.Prop);
            }

            public class HasWritableIndexer
            {
                public string NormalProp { get; set; }

                public object this[int index]
                {
                    get => null;
                    // not actually testing the set -- just expect DeepClone not to break on the indexer
                    // ReSharper disable once ValueParameterNotUsed
                    set { }
                }
            }

            [Test]
            public void ShouldCloneAObjectWithAWritableIndexer()
            {
                // Arrange
                var src = new HasWritableIndexer()
                {
                    NormalProp = "value"
                };
                // Pre-Assert
                // Act
                var result = src.DeepClone();
                // Assert
                Expect(result).Not.To.Be.Null();
                Expect(result.NormalProp).To.Equal(src.NormalProp);
            }
        }

        [TestFixture]
        public class AsArray
        {
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
        }

        [TestFixture]
        public class IntersectionEquality
        {
            [Test]
            public void GivenSourceAndDestWithMatchingOverlappingProps_ShouldReturnTrue()
            {
                //--------------- Arrange -------------------
                var left = new
                {
                    foo = "bar",
                    name = "Mickey"
                };
                var right = new
                {
                    quuz = "wibbles",
                    name = "Mickey"
                };

                //--------------- Assume ----------------

                //--------------- Act ----------------------
                var result = left.DeepIntersectionEquals(right);

                //--------------- Assert -----------------------
                Expect(result).To.Be.True();
            }

            [Test]
            public void GivenSourceAndDestWithNoOverlappingProps_ShouldReturnFalse()
            {
                //--------------- Arrange -------------------
                var left = new
                {
                    foo = "bar",
                };
                var right = new
                {
                    quuz = "wibbles",
                };

                //--------------- Assume ----------------

                //--------------- Act ----------------------
                var result = left.DeepIntersectionEquals(right);

                //--------------- Assert -----------------------
                Expect(result).To.Be.False();
            }

            [TestFixture]
            public class OperatingOnCollections
            {
                [Test]
                public void WhenCollectionItemWithMismatchedProperties_WhenSamePropertiesMatch_ShouldReturnTrue()
                {
                    //--------------- Arrange -------------------
                    var collection1 = new[]
                    {
                        new
                        {
                            id = 1,
                            name = "Bob"
                        }
                    };

                    //--------------- Assume ----------------

                    //--------------- Act ----------------------
                    var result = collection1.ContainsOneIntersectionEqualTo(new {id = 1});

                    //--------------- Assert -----------------------
                    Expect(result).To.Be.True();
                }

                [Test]
                public void WhenCollectionItemWithMismatchedProperties_WhenSamePropertiesDoNotMatch_ShouldReturnFalse()
                {
                    //--------------- Arrange -------------------
                    var collection1 = new[]
                    {
                        new
                        {
                            id = 1,
                            name = "Bob"
                        }
                    };

                    //--------------- Assume ----------------

                    //--------------- Act ----------------------
                    var result = collection1.ContainsOneIntersectionEqualTo(new {id = 2});

                    //--------------- Assert -----------------------
                    Expect(result).To.Be.False();
                }

                [Test]
                public void
                    WhenCollectionItemWithMismatchedIgnoredProperties_WhenSamePropertiesDoNotMatch_ShouldReturnTrue()
                {
                    //--------------- Arrange -------------------
                    var collection1 = new[]
                    {
                        new
                        {
                            id = 1,
                            name = "Bob"
                        }
                    };

                    //--------------- Assume ----------------

                    //--------------- Act ----------------------
                    var result = collection1.ContainsOneIntersectionEqualTo(
                        new
                        {
                            id = 2,
                            name = "Bob"
                        },
                        "id");

                    //--------------- Assert -----------------------
                    Expect(result).To.Be.True();
                }

                [Test]
                public void WhenCollectionItemWithAllMismatchedProperties_ShouldReturnFalse()
                {
                    //--------------- Arrange -------------------
                    var collection1 = new[]
                    {
                        new
                        {
                            id = 1,
                            name = "Bob"
                        }
                    };

                    //--------------- Assume ----------------

                    //--------------- Act ----------------------
                    var result = collection1.ContainsOneIntersectionEqualTo(new {value = 1});

                    //--------------- Assert -----------------------
                    Expect(result).To.Be.False();
                }
            }
        }

        [TestFixture]
        public class SubEqualityTesting
        {
            [Test]
            public void WhenDestinationHasMorePropertiesButSameNamedOnesMatch_ReturnsTrue()
            {
                //---------------Set up test pack-------------------
                var rs = GetRandomString();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = (new {prop = rs}).DeepSubEquals(
                    new
                    {
                        prop = rs,
                        bar = GetRandomString()
                    });

                //---------------Test Result -----------------------
                Assert.IsTrue(result);
            }

            [Test]
            public void WhenDestinationIsMissingProperty_ReturnsFalse()
            {
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = (new {prop = GetRandomString()})
                    .DeepSubEquals(new object());

                //---------------Test Result -----------------------
                Assert.IsFalse(result);
            }

            [Test]
            public void GivenSourceAndDestWithNoOverlappingProps_ShouldReturnFalse()
            {
                //--------------- Arrange -------------------
                var left = new
                {
                    foo = "bar"
                };
                var right = new
                {
                    quuz = "wibbles"
                };

                //--------------- Assume ----------------

                //--------------- Act ----------------------
                var result = left.DeepSubEquals(right);

                //--------------- Assert -----------------------
                Expect(result).To.Be.False();
            }
        }

        [TestFixture]
        public class AsEnumerable
        {
            [Test]
            public void ShouldBeAbleToEnumerateIEnumerable()
            {
                // Arrange
                var data = new[] { 1, 2, 3 };
                // Act
                var result = data.AsEnumerable<long>().ToArray();
                // Assert
                Expect(result).To.Equal(data.Select(o => (long)o));
            }

            [Test]
            public void ShouldBeAbleToEnumerateDuckable()
            {
                // Arrange
                var data = new[] { 4, 5, 6 };
                var enumerable = new MyEnumerable<int>(data);
                // Act
                var result = enumerable.AsEnumerable<int>();
                // Assert
                Expect(result).To.Equal(data);
            }

            [Test]
            public void ShouldBeAbleToEnumerateParseable()
            {
                // Arrange
                var data = new[] { "7", "8", "9" };
                // Act
                var result = data.AsEnumerable<int>();
                // Assert
                Expect(result).To.Equal(new[] { 7, 8, 9 });
            }
        }

        [TestFixture]
        public class TruncateTo
        {
            [TestFixture]
            public class OperatingOnDecimal
            {
                [Test]
                public void ShouldTruncateToRequestedPlaces()
                {
                    // Arrange
                    var value = 1.234M;
                    var expected = 1.23M;
                    // Act
                    var result = value.TruncateTo(2);
                    // Assert
                    Expect(result)
                        .To.Equal(expected);
                }

                [Test]
                public void ShouldNotRound()
                {
                    // Arrange
                    var value = 1.555M;
                    var expected = 1.55M;
                    
                    // Act
                    var result = value.TruncateTo(2);
                    // Assert
                    Expect(result)
                        .To.Equal(expected);
                }
            }
            
            [TestFixture]
            public class OperatingOnDouble
            {
                [Test]
                public void ShouldTruncateToRequestedPlaces()
                {
                    // Arrange
                    var value = 1.234D;
                    var expected = 1.23D;
                    // Act
                    var result = value.TruncateTo(2);
                    // Assert
                    Expect(result)
                        .To.Equal(expected);
                }

                [Test]
                public void ShouldNotRound()
                {
                    // Arrange
                    var value = 1.555D;
                    var expected = 1.55D;
                    
                    // Act
                    var result = value.TruncateTo(2);
                    // Assert
                    Expect(result)
                        .To.Equal(expected);
                }
            }
        }

        [TestFixture]
        public class ToFixed
        {
            [TestFixture]
            public class OperatingOnDecimal
            {
                [Test]
                public void ShouldRoundDownAsRequired()
                {
                    // Arrange
                    var value = 1.234M;
                    var expected = 1.23M;
                    // Act
                    var result = value.ToFixed(2);
                    // Assert
                    Expect(result)
                        .To.Equal(expected);
                }

                [Test]
                public void ShouldRoundUpAsRequired()
                {
                    // Arrange
                    var value = 1.555M;
                    var expected = 1.56M;
                    
                    // Act
                    var result = value.ToFixed(2);
                    // Assert
                    Expect(result)
                        .To.Equal(expected);
                }
            }
            
            [TestFixture]
            public class OperatingOnDouble
            {
                [Test]
                public void ShouldRoundDownAsRequired()
                {
                    // Arrange
                    var value = 1.234D;
                    var expected = 1.23D;
                    // Act
                    var result = value.ToFixed(2);
                    // Assert
                    Expect(result)
                        .To.Equal(expected);
                }

                [Test]
                public void ShouldRoundUpAsRequired()
                {
                    // Arrange
                    var value = 1.555D;
                    var expected = 1.56D;
                    
                    // Act
                    var result = value.ToFixed(2);
                    // Assert
                    Expect(result)
                        .To.Equal(expected);
                }
            }
        }

        [Test]
        public void ShouldBeAbleToRetrieveNullValue()
        {
            // Arrange
            var data = new { name = null as string };
            
            // Act
            var result = data.Get<string>("name");
            // Assert
            Expect(result)
                .To.Be.Null();
        }

        public class Complex<T>
        {
            public Simple<T> prop { get; set; }

            public Complex()
            {
                prop = new Simple<T>();
            }
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


        public class ThingWithCollection<T>
        {
            public IEnumerable<T> Collection { get; set; }
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
            string MealPreferences { get; set; }
            string TravelPreferences { get; set; }
        }

        public interface ITraveller : IActor, ITravellerDetails
        {
        }

        public class ActualTravellerDetails : ITravellerDetails
        {
            public string IdNumber { get; set; }
            public string[] PassportNumbers { get; set; }
            public string MealPreferences { get; set; }
            public string TravelPreferences { get; set; }
        }

        public class HasAnArrayOfStrings
        {
            public string[] Strings { get; set; }
        }


        public class HasAnEnumerable
        {
            public IEnumerable<string> Stuff { get; set; }
        }

        public class HasAListOfStrings
        {
            public List<string> Strings { get; set; }
        }

        public class HasAListOfDates
        {
            public List<DateTime> DateTimes { get; set; }
        }

        public class HasIdAndName
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class Traveller : ITraveller
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

        [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
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