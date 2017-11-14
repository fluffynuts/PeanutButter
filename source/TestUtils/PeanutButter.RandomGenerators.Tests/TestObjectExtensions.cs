using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using static NExpect.Expectations;
// ReSharper disable PossibleMultipleEnumeration

namespace PeanutButter.RandomGenerators.Tests
{
    [TestFixture]
    public class TestObjectExtensions
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
            // TODO: swap this out when NExpect has InstanceOf<> support
            Assert.That(result, Is.InstanceOf<EmptyType>());
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
            PropertyAssert.AreDeepEqual(src, result);
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
            public IEnumerable<Node> Nodes { get; set ; }
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
            public List<Node> Nodes { get; set ; }
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

        public static IEnumerable<Node> CollectionOfNodes() {
            yield return new Node() { Flag = true, Guid = Guid.Parse("CBCEAB18-6A92-48D3-A699-998D7BB8DA0E"), Id = 1, Name = "one" };
            yield return new Node() { Flag = false, Guid = Guid.Parse("23B63474-911E-409A-9C23-D8A2A8324FAA"), Id = 2, Name = "two" };
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

    }
}