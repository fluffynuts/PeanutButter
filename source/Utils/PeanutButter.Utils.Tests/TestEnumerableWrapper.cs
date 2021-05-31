using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using static NExpect.Expectations;

namespace PeanutButter.Utils.Tests
{
    // .net does duck-typed enumeration on things which implement
    // the correct methods. This wrapper is to facilitate the same
    // when the incoming type may or may not be enumerable, gracefully
    // falling back on empty results
    [TestFixture]
    public class TestEnumerableWrapper
    {
        [TestFixture]
        public class TestInbuiltDuckTypedEnumeration
        {
            [Test]
            public void ShouldBeAbleToEnumerate()
            {
                // Arrange
                var data = new[] { 1, 2, 3 };
                var enumerable = new MyEnumerable<int>(data);
                var collector = new List<int>();
                // Act
                foreach (var item in enumerable)
                {
                    collector.Add((int) item);
                }

                // Assert
                Expect(collector).To.Equal(data);
            }
        }

        [TestFixture]
        public class TestWrappers
        {
            [Test]
            public void ShouldBeAbleToEnumerateOverEnumerable()
            {
                // Arrange
                var data = new[] { 1, 2, 3 };
                var collector = new List<int>();
                var firstLayer = new MyEnumerable<int>(data);
                var sut = new EnumerableEnumerableWrapper(firstLayer);
                // Act
                Expect(sut.IsValid).To.Be.True();
                foreach (var item in sut)
                {
                    collector.Add((int) item);
                }

                // Assert
                Expect(collector).To.Equal(data);
            }

            [Test]
            public void ShouldBeAbleToEnumerateOverEnumerable_Typed()
            {
                // Arrange
                var data = new[] { 1, 2, 3 };
                var firstLayer = new MyEnumerable<int>(data);
                var sut = new EnumerableEnumerableWrapper<int>(firstLayer);
                // Act
                Expect(sut.IsValid).To.Be.True();
                var result = sut.ToList();
                // Assert
                Expect(result).To.Equal(data);
            }
        }
    }
}