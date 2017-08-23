using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using static NExpect.Expectations;

namespace PeanutButter.RandomGenerators.Tests
{
    [TestFixture]
    public class TestObjectExtensions
    {
        public class EmptyType
        {
        }

        [Test]
        public void DeepClone_GivenEmptyObject_ShouldReturnNewEmptyObject()
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
        public void DeepClone_ShouldCloneFirstLevel()
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
        public void DeepClone_ShouldCloneSecondLevelButNotChildRefs()
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
        public void DeepClone_ShouldCloneAnArray()
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
        public void DeepClone_ShouldCloneAnIEnumerable()
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
        public void DeepClone_ShouldCloneAGenericList()
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
        public void DeepClone_ShouldCloneANullGenericList()
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
    }
}