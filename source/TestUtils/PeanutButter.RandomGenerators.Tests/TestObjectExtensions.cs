using System;
using NUnit.Framework;
using PeanutButter.TestUtils.Generic;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.RandomGenerators.Tests
{
    [TestFixture]
    public class TestObjectExtensions : AssertionHelper
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
            Expect(result, Is.Not.Null);
            Expect(result, Is.InstanceOf<EmptyType>());
            Expect(result.GetType().GetProperties(), Is.Empty);
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
            Expect(src.Child, Is.Not.Null);
            // Act
            var result = src.DeepClone();
            // Assert
            Expect(result.Child, Is.Not.EqualTo(src.Child));
            PropertyAssert.AreDeepEqual(src, result);
        }
    }
}