using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using NUnit.Framework;
using PeanutButter.Utils.Dictionaries;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnassignedField.Global

namespace PeanutButter.Utils.Tests.Dictionaries
{
    [TestFixture]
    public class TestDictionaryWrappingObject
    {
        [TestCase(typeof(IDictionary<string, object>))]
        public void ShouldImplementIDictionary_string_object(Type expected)
        {
            // Arrange
            var sut = typeof(DictionaryWrappingObject);
            // Pre-Assert
            // Act
            Expect(sut)
                .To.Implement(expected);
            // Assert
        }

        [Test]
        public void ShouldProvideNullPatternForNullValue()
        {
            // Arrange
            var sut = Create(null);
            // Act
            Expect(sut.Keys)
                .To.Be.Empty();
            // Assert
        }

        [Test]
        public void ShouldBeAbleToGetExistingPropertyValue_CaseSensitive()
        {
            // Arrange
            var src = new { Prop = GetRandomString() };
            var sut = Create(src);
            // Pre-Assert
            // Act
            var result = sut["Prop"];
            // Assert
            Expect(result).To.Equal(src.Prop);
        }

        [Test]
        public void ShouldBeAbleToGetExistingPropertyValue_CaseInSensitive()
        {
            // Arrange
            var src = new { Prop = 1 };
            var sut = Create(src, StringComparer.OrdinalIgnoreCase);
            // Pre-Assert
            // Act
            var result = sut["proP"];
            // Assert
            Expect(result).To.Equal(1);
        }

        [Test]
        public void ShouldThrowWhenIndexingNonExistentProperty()
        {
            // Arrange
            var src = new { Moo = "Cow" };
            var sut = Create(src);
            // Pre-Assert
            // Act
            Expect(() => sut["Cow"])
                .To.Throw<KeyNotFoundException>()
                .With.Message.Containing("Cow");
            // Assert
        }

        public class HasAField
        {
            public string Name;
        }

        [Test]
        public void ShouldBeAbleToGetExistingFieldValue()
        {
            // Arrange
            var src = new HasAField() { Name = GetRandomString() };
            var sut = Create(src);
            // Pre-Assert
            // Act
            var result = sut["Name"];
            // Assert
            Expect(result).To.Equal(src.Name);
        }

        [Test]
        public void ShouldThrowWhenIndexingNonExistentField()
        {
            // Arrange
            var src = GetRandom<HasAField>();
            var sut = Create(src);
            // Pre-Assert
            // Act
            Expect(() => sut["Cow"])
                .To.Throw<KeyNotFoundException>()
                .With.Message.Containing("Cow");
            // Assert
        }

        public class HasAProperty
        {
            public string Prop { get; set; }
        }

        [Test]
        public void ShouldBeAbleToSetAnExistingWritablePropertyWithTheCorrectType()
        {
            // Arrange
            var src = GetRandom<HasAProperty>();
            var sut = Create(src);
            var expected = GetAnother(src.Prop);
            // Pre-Assert
            // Act
            sut["Prop"] = expected;
            // Assert
            Expect(sut["Prop"]).To.Equal(expected);
            Expect(src.Prop).To.Equal(expected);
        }

        [Test]
        public void Keys_ShouldReturnAllKeys()
        {
            // Arrange
            var src = new { One = 1, Two = "two" };
            var expected = new[] { "One", "Two" };
            var sut = Create(src);
            // Pre-Assert
            // Act
            var result = sut.Keys;
            // Assert
            Expect(result).To.Be.Deep.Equivalent.To(expected);
        }

        [Test]
        public void Values_ShouldReturnAllValues()
        {
            // Arrange
            var src = new { One = 1, Two = "two" };
            var expected = new object[] { 1, "two" };
            var sut = Create(src);
            // Pre-Assert
            // Act
            var result = sut.Values;
            // Assert
            Expect(result).To.Be.Deep.Equivalent.To(expected);
        }

        public class HasAPropertyAndField
        {
            public string Field;
            public bool Property { get; set; }
        }

        [Test]
        public void Count_ShouldReturnPropertyAndFieldCount()
        {
            // Arrange
            var src = GetRandom<HasAPropertyAndField>();
            var sut = Create(src);
            // Pre-Assert
            // Act
            var result = sut.Count;
            // Assert
            Expect(result).To.Equal(2);
        }

        [Test]
        public void IsReadOnly_ShouldReturnFalse()
        {
            // Arrange
            var sut = Create();
            // Pre-Assert
            // Act
            var result = sut.IsReadOnly;
            // Assert
            Expect(result).To.Be.False();
        }

        [TestFixture]
        public class AddingAndRemovingItems
        {
            [Test]
            public void Add_ShouldThrowInvalidOperationException()
            {
                // Arrange
                var sut = Create();

                // Pre-Assert

                // Act
                Expect(() => sut.Add(GetRandom<KeyValuePair<string, object>>()))
                    .To.Throw<InvalidOperationException>();

                Expect(() => sut.Add(GetRandomString(), new object()))
                    .To.Throw<InvalidOperationException>();

                // Assert
            }

            [Test]
            public void Clear_ShouldThrowInvalidOperationException()
            {
                // Arrange
                var sut = Create();

                // Pre-Assert

                // Act
                Expect(() => sut.Clear())
                    .To.Throw<InvalidOperationException>();

                // Assert
            }

            [Test]
            public void Remove_ShouldThrowInvalidOperationException()
            {
                // Arrange
                var sut = Create();

                // Pre-Assert

                // Act
                Expect(() => sut.Remove(GetRandomString()))
                    .To.Throw<InvalidOperationException>();

                Expect(() => sut.Remove(GetRandom<KeyValuePair<string, object>>()))
                    .To.Throw<InvalidOperationException>();

                // Assert
            }
        }

        [TestFixture]
        public class Contains
        {
            [Test]
            public void WhenPropertyExistsWithProvidedValue_ShouldReturnTrue()
            {
                // Arrange
                var src = new { Wat = "BatMan!" };
                var sut = Create(src);

                // Pre-Assert

                // Act
                var result = sut.Contains(new KeyValuePair<string, object>("Wat", "BatMan!"));

                // Assert
                Expect(result).To.Be.True();
            }

            [Test]
            public void WhenPropertyExistsWithDifferentValue_ShouldReturnFalse()
            {
                // Arrange
                var src = new { Wat = "BatMan!" };
                var sut = Create(src);

                // Pre-Assert

                // Act
                var result = sut.Contains(new KeyValuePair<string, object>("Wat", "CowMan!"));

                // Assert
                Expect(result).To.Be.False();
            }

            [Test]
            public void WhenValueExistsOnDifferentProperty_ShouldReturnFalse()
            {
                // Arrange
                var src = new { Wat = "BatMan!" };
                var sut = Create(src);

                // Pre-Assert

                // Act
                var result = sut.Contains(new KeyValuePair<string, object>("Moo", "BatMan!"));

                // Assert
                Expect(result).To.Be.False();
            }
        }

        [TestFixture]
        public class ContainsKey
        {
            [Test]
            public void WhenDoesHaveKey_ShouldReturnTrue()
            {
                // Arrange
                var src = new { Cake = "Yummy" };
                var sut = Create(src);

                // Pre-Assert

                // Act
                var result = sut.ContainsKey("Cake");

                // Assert
                Expect(result).To.Be.True();
            }

            [Test]
            public void WhenDoesNotHaveKey_ShouldReturnTrue()
            {
                // Arrange
                var src = new { Cake = "Yummy" };
                var sut = Create(src);

                // Pre-Assert

                // Act
                var result = sut.ContainsKey("Moofins");

                // Assert
                Expect(result).To.Be.False();
            }
        }

        [TestFixture]
        public class TryGetValue
        {
            [Test]
            public void WhenKeyExists_ShouldReturnTrue_AndOutValue()
            {
                // Arrange
                var src = new { Id = 1, Name = "Sheldon" };
                var sut = Create(src);

                // Pre-Assert

                // Act
                var result = sut.TryGetValue("Name", out var value);

                // Assert
                Expect(result).To.Be.True();
                Expect(value).To.Equal(src.Name);
            }

            [Test]
            public void WhenKeyDoesNotExist_ShouldReturnFalse_AndOutValueNull()
            {
                // Arrange
                var src = new { Id = 1, Name = "Sheldon" };
                var sut = Create(src);

                // Pre-Assert

                // Act
                var result = sut.TryGetValue("Name2", out var value);

                // Assert
                Expect(result).To.Be.False();
                Expect(value).To.Be.Null();
            }
        }

        [TestFixture]
        public class CopyTo
        {
            [Test]
            public void WhenSrcIsEmpty_ShouldCopyNothing()
            {
                // Arrange
                var src = new object();
                var sut = Create(src);
                var target = new KeyValuePair<string, object>[0];

                // Pre-Assert

                // Act
                Expect(() => sut.CopyTo(target, 0)).Not.To.Throw();

                // Assert
            }

            [Test]
            public void WhenSrcIsNotEmpty_ShouldCopyOutPropertiesAndFields()
            {
                // Arrange
                var src = GetRandom<HasAPropertyAndField>();
                var sut = Create(src);
                var target = new KeyValuePair<string, object>[3];

                // Pre-Assert

                // Act
                sut.CopyTo(target, 1);

                // Assert
                Expect(target[0]).To.Equal(default(KeyValuePair<string, object>));
                Expect(target).To.Contain.Exactly(1)
                    .Equal.To(new KeyValuePair<string, object>(
                        nameof(HasAPropertyAndField.Field),
                        src.Field));
                Expect(target).To.Contain.Exactly(1)
                    .Equal.To(new KeyValuePair<string, object>(
                        nameof(HasAPropertyAndField.Property),
                        src.Property));
            }
        }

        [TestFixture]
        public class Enumeration
        {
            [Test]
            public void ShouldWithstandAForeach()
            {
                // Arrange
                var src = new { Id = Guid.NewGuid(), Description = "Pizza" };
                var sut = Create(src);
                var collector = new List<Tuple<string, object>>();

                // Pre-Assert

                // Act
                foreach (var kvp in sut)
                {
                    collector.Add(Tuple.Create(kvp.Key, kvp.Value));
                }

                // Assert
                Expect(collector).To.Contain.Only(2).Items();
                Expect(collector).To.Contain.Exactly(1)
                    .Equal.To(Tuple.Create(nameof(src.Id), src.Id as object));
                Expect(collector).To.Contain.Exactly(1)
                    .Equal.To(Tuple.Create(nameof(src.Description), src.Description as object));
            }
        }

        [TestFixture]
        public class WhenConstructedToProvideWrappedObjectsForComplexProperties
        {
            [Test]
            public void ShouldNotWrapPrimitiveProperty()
            {
                // Arrange
                var data = new { id = 1 };
                var sut = Create(data, wrapRecursively: true);
                // Act
                var result = sut["id"];
                // Assert
                Expect(result)
                    .To.Be.An.Instance.Of<int>();
            }

            [Test]
            public void ShouldWrapAComplexObject()
            {
                // Arrange
                var data = new { sub = new { id = 1 } };
                var sut = Create(data, wrapRecursively: true);
                // Act
                var result = sut["sub"];
                // Assert
                Expect(result)
                    .To.Be.An.Instance.Of<IDictionary<string, object>>();
                Expect(result.AsDict<string, object>()["id"])
                    .To.Equal(1);
            }

            [Test]
            public void ShouldReturnTheSameWrapperEachTime()
            {
                // Arrange
                var data = new { sub = new { id = 1 } };
                var sut = Create(data, wrapRecursively: true);
                // Act
                var result1 = sut["sub"];
                var result2 = sut["sub"];
                // Assert
                Expect(result1)
                    .To.Be(result2);
            }

            [Test]
            public void ShouldReturnTheSameWrapperForRecursiveMembers()
            {
                // Arrange
                var node = new LinkedListItem();
                node.Previous = node;
                var sut = Create(node, wrapRecursively: true);
                // Act
                var result1 = sut["Previous"] as IDictionary<string, object>;
                var result2 = result1["Previous"] as IDictionary<string, object>;
                // Assert
                Expect(result1)
                    .To.Be(result2);
            }

            [Test]
            public void ShouldPresentACollectionAsACollectionOfWrappers()
            {
                // Arrange
                var data = new
                {
                    nodes = new[]
                    {
                        new { id = 1, text = "one" },
                        new { id = 2, text = "two" }
                    }
                };
                var sut = Create(data, wrapRecursively: true);
                // Act
                var nodes = sut["nodes"] as IEnumerable<IDictionary<string, object>>;
                // Assert
                Expect(nodes)
                    .Not.To.Be.Null();
                var nodeArray = nodes.ToArray();
                Expect(nodeArray[0]["id"])
                    .To.Equal(1);
                Expect(nodeArray[1]["id"])
                    .To.Equal(2);
            }

            [Test]
            public void ShouldReuseWrappersForRecursivelyReferencedCollections()
            {
                // Arrange
                // root -> [ child ] -> [ root ] -> [ child ] ...
                var root = new Node() { Name = "root" };
                var child = new Node() { Name = "child" };
                root.Children = new[] { child };
                child.Children = new[] { root };
                // Act
                var sut = Create(root, wrapRecursively: true);
                // Assert
                var firstLevelChildren = (sut["Children"] as IEnumerable<IDictionary<string, object>>).ToArray();
                var secondLevelChildren =
                    (firstLevelChildren[0]["Children"] as IEnumerable<IDictionary<string, object>>).ToArray();
                var thirdLevelChildren =
                    (secondLevelChildren[0]["Children"] as IEnumerable<IDictionary<string, object>>).ToArray();
                Expect(secondLevelChildren[0]["Name"])
                    .To.Equal("root");
                Expect(secondLevelChildren[0])
                    .To.Be(sut);
                Expect(firstLevelChildren[0]["Name"])
                    .To.Equal("child");
                Expect(firstLevelChildren[0])
                    .To.Be(thirdLevelChildren[0]);
            }

            [TestFixture]
            public class Writeback
            {
                [Test]
                public void ShouldBeAbleToWriteBackToRootObjectProperties()
                {
                    // Arrange
                    var node = GetRandom<Node>();
                    var sut = Create(node, wrapRecursively: true);
                    var expected = Guid.NewGuid();
                    // Act
                    sut["Id"] = expected;
                    // Assert
                    Expect(node.Id)
                        .To.Equal(expected);
                }

                [Test]
                public void ShouldBeAbleToWriteBackToFirstLevelObjectProperties()
                {
                    // Arrange
                    var item = new LinkedListItem() { Name = GetRandomString() };
                    var next = new LinkedListItem() { Name = GetRandomString() };
                    item.Next = next;
                    var sut = Create(item, wrapRecursively: true);
                    var expected = GetRandomString(20);
                    // Act
                    var wrappedNext = sut["Next"] as IDictionary<string, object>;
                    wrappedNext["Name"] = expected;
                    // Assert
                    Expect(next.Name)
                        .To.Equal(expected);
                }

                [Test]
                public void ShouldBeAbleToWriteToPropertiesOfCollectionItem()
                {
                    // Arrange
                    var root = GetRandom<Node>();
                    var child = GetRandom<Node>();
                    root.Children = new[] { child };
                    var expected = GetRandomString(20);
                    var sut = Create(root, wrapRecursively: true);
                    // Act
                    var wrappedChildren = sut["Children"].AsDictArray<string, object>();
                    wrappedChildren[0]["Name"] = expected;
                    // Assert
                    Expect(child.Name)
                        .To.Equal(expected);
                }

                [Test]
                public void ShouldBeAbleToReplaceChildWithRawObject()
                {
                    // Arrange
                    var root = GetRandom<LinkedListItem>();
                    var original = GetRandom<LinkedListItem>();
                    root.Next = original;
                    var replacement = GetRandom<LinkedListItem>();
                    var sut = Create(root, wrapRecursively: true);
                    // Act
                    sut["Next"] = replacement;
                    var result = sut["Next"];
                    // Assert
                    Expect(result)
                        .To.Be.An.Instance.Of<IDictionary<string, object>>();
                    var dict = result.AsDict<string, object>();
                    Expect(dict["Name"])
                        .To.Equal(replacement.Name);
                }

                [Test]
                public void ShouldAutomaticallyHandleWrappedWritebackWithCorrectWrappedType()
                {
                    // Arrange
                    var root = GetRandom<LinkedListItem>();
                    var original = GetRandom<LinkedListItem>();
                    root.Next = original;
                    var replacement = GetRandom<LinkedListItem>();
                    var sut = Create(root, wrapRecursively: true);
                    var wrappedReplacement = Create(replacement, wrapRecursively: true);

                    // Act
                    Expect(() =>
                    {
                        sut["Next"] = wrappedReplacement;
                    }).Not.To.Throw();
                    // Assert
                    Expect(root.Next)
                        .To.Be(replacement);
                }

                [Test]
                public void ShouldThrowInformativeErrorWhenAttemptingToWriteInvalidType()
                {
                    // Arrange
                    var root = GetRandom<LinkedListItem>();
                    var next = GetRandom<Node>();
                    var sut = Create(root, wrapRecursively: true);
                    // Act
                    Expect(() =>
                        {
                            sut["Next"] = next;
                        }).To.Throw<ArgumentException>()
                        .With.Message.Containing("Attempt to set property 'Next'")
                        .Then("will fail")
                        .Then("target type")
                        .Then(nameof(Node));
                    // Assert
                }
            }
        }

        [TestFixture]
        public class Unwrapping
        {
            [TestCase(typeof(IWrapper))]
            public void ShouldImplement_(Type expected)
            {
                // Arrange
                var sut = typeof(DictionaryWrappingObject);
                // Act
                Expect(sut)
                    .To.Implement(expected);
                // Assert
            }

            [Test]
            public void ShouldBeAbleToObtainOriginalObject()
            {
                // Arrange
                var root = GetRandom<Node>();
                var sut = Create(root) as IWrapper;
                // Act
                var wrapped = sut.Unwrap();
                // Assert
                Expect(wrapped)
                    .To.Be(root);
            }

            [Test]
            public void ShouldBeAbleToObtainOriginalObjectWithType()
            {
                // Arrange
                var root = GetRandom<Node>();
                var sut = Create(root) as IWrapper;
                // Act
                var wrapped = sut.Unwrap<Node>();
                // Assert
                Expect(wrapped)
                    .To.Be(root);
            }

            [Test]
            public void ShouldBeAbleToObtainOriginalObjectWithBaseType()
            {
                // Arrange
                var root = GetRandom<DerivativeNode>();
                var sut = Create(root) as IWrapper;
                // Act
                var wrapped = sut.Unwrap<Node>();
                // Assert
                Expect(wrapped)
                    .To.Be(root);
            }

            [Test]
            public void ShouldGracefullyFailUnwrapWithTryUnwrap()
            {
                // Arrange
                var root = GetRandom<Node>();
                var sut = Create(root) as IWrapper;
                // Act
                var result = sut.TryUnwrap<LinkedListItem>(out var wrapped);
                // Assert
                Expect(result)
                    .To.Be.False();
                Expect(wrapped)
                    .To.Be.Null();
            }
        }

        [TestFixture]
        public class SpecialCases
        {
            [TestFixture]
            public class WrappingGenericDictionaryWithStringKeys
            {
                [Test]
                public void ShouldBeAbleToReadAndWriteValue()
                {
                    // Arrange
                    var expected1 = GetRandomInt();
                    var expected2 = GetAnother(expected1);
                    var wrapped = new Dictionary<string, int>()
                    {
                        ["id"] = expected1
                    };
                    var foo = wrapped.Values.FirstOrDefault();
                    var sut = Create(wrapped);
                    // Act
                    var result1 = sut["id"];
                    sut["id"] = expected2;
                    var result2 = sut["id"];
                    // Assert
                    Expect(result1)
                        .To.Equal(expected1);
                    Expect(result2)
                        .To.Equal(expected2);
                }
            }

            [TestFixture]
            public class WrappingNameValueCollection
            {
                [Test]
                public void ShouldBeAbleToReadAndWriteValue()
                {
                    // Arrange
                    var expected1 = GetRandomString();
                    var expected2 = GetAnother(expected1);
                    var wrapped = new NameValueCollection()
                    {
                        ["id"] = expected1
                    };
                    var sut = Create(wrapped);
                    // Act
                    var result1 = sut["id"];
                    sut["id"] = expected2;
                    var result2 = sut["id"];
                    // Assert
                    Expect(result1)
                        .To.Equal(expected1);
                    Expect(result2)
                        .To.Equal(expected2);
                }
            }

            [TestFixture]
            public class WrappingConnectionStringSettingsCollection
            {
                [Test]
                public void ShouldBeAbleToReadAndWriteValue()
                {
                    // Arrange
                    var expected1 = GetRandomString();
                    var expected2 = GetAnother(expected1);
                    var wrapped = new ConnectionStringSettingsCollection
                    {
                        new ConnectionStringSettings("id", expected1)
                    };
                    var sut = Create(wrapped);
                    // Act
                    var result1 = sut["id"];
                    sut["id"] = expected2;
                    var result2 = sut["id"];
                    // Assert
                    Expect(result1)
                        .To.Equal(expected1);
                    Expect(result2)
                        .To.Equal(expected2);
                }
            }
        }

        private static DictionaryWrappingObject Create(
            object wrapped = null,
            StringComparer stringComparer = null,
            bool wrapRecursively = false
        )
        {
            return new DictionaryWrappingObject(
                wrapped,
                stringComparer ?? StringComparer.Ordinal,
                wrapRecursively
            );
        }

        public class DerivativeNode : Node
        {
        }

        public class Node
        {
            public Guid Id { get; set; } = Guid.NewGuid();
            public Node Parent { get; set; }
            public Node[] Children { get; set; }
            public string Name { get; set; }
        }

        public class LinkedListItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public LinkedListItem Previous { get; set; }
            public LinkedListItem Next { get; set; }
        }
    }


    public static class DictionaryCastExtensions
    {
        public static IDictionary<TKey, TValue> AsDict<TKey, TValue>(
            this object obj
        )
        {
            return obj as IDictionary<TKey, TValue>;
        }

        public static IDictionary<TKey, TValue>[] AsDictArray<TKey, TValue>(
            this object obj
        )
        {
            return (obj as IEnumerable<IDictionary<TKey, TValue>>)?.ToArray();
        }
    }
}