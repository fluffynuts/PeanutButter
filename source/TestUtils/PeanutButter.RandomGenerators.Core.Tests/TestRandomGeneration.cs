using System;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.RandomGenerators.Core.Tests
{
    [TestFixture]
    public class TestRandomGeneration
    {
        [Test]
        public void ShouldBeAbleToBuildComplexObjects()
        {
            // Arrange
            var item = GetRandom<Simple>();

            // Pre-Assert

            // Act
            Assert.That(item, Is.Not.Null);

            // Assert
        }

        public class Simple
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}