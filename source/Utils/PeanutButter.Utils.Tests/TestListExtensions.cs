using System;
using System.Collections.Generic;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using static NExpect.Expectations;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestListExtensions
    {
        [TestFixture]
        public class Shift
        {
            [Test]
            public void ShouldReturnFirstValueFromList()
            {
                // Arrange
                var list = new List<int>() { 1, 2, 3 };
                // Act
                var result = list.Shift();
                // Assert
                Expect(result)
                    .To.Equal(1);
            }

            [Test]
            public void ShouldRemoveTheItemFromTheList()
            {
                // Arrange
                var list = new List<int>() { 1, 1, 1 };
                // Act
                var result = list.Shift();
                // Assert
                Expect(result)
                    .To.Equal(1);
                Expect(list)
                    .To.Equal(new[] { 1, 1 });
            }

            [Test]
            public void ShouldThrowWhenNoElements()
            {
                // Arrange
                var list = new List<int>();
                // Act
                Expect(() => list.Shift())
                    .To.Throw<InvalidOperationException>()
                    .With.Message.Containing("List contains no elements");
                // Assert
            }
        }

        [TestFixture]
        public class Pop
        {
            [Test]
            public void ShouldReturnLastValueFromList()
            {
                // Arrange
                var list = new List<string>() { "a", "b", "c" };
                // Act
                var result = list.Pop();
                // Assert
                Expect(result)
                    .To.Equal("c");
            }

            [Test]
            public void ShouldRemoveValueFromList()
            {
                // Arrange
                var list = new List<string>() { "a", "b", "c" };
                // Act
                var result = list.Pop();
                // Assert
                Expect(result)
                    .To.Equal("c");
                Expect(list)
                    .To.Equal(new[] { "a", "b" });
            }

            [Test]
            public void ShouldThrowIfListIsEmpty()
            {
                // Arrange
                var list = new List<string>();
                // Act
                Expect(() => list.Pop())
                    .To.Throw<InvalidOperationException>()
                    .With.Message.Containing("List contains no elements");
                // Assert
            }
        }

        [TestFixture]
        public class Unshift
        {
            [Test]
            public void ShouldAddElementToStartOfList()
            {
                // Arrange
                var list = new List<int>() { 1, 2, 3 };
                // Act
                list.Unshift(4);
                // Assert
                Expect(list)
                    .To.Equal(new[] { 4, 1, 2, 3 });
            }
        }

        [TestFixture]
        public class Push
        {
            [Test]
            public void ShadowsAddForCompleteness()
            {
                // Arrange
                var list = new List<bool>() { true, false };
                // Act
                list.Push(true);
                // Assert
                Expect(list)
                    .To.Equal(new[] { true, false, true });
            }
        }

    }
}