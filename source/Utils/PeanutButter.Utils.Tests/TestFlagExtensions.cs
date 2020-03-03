using System.Reflection;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using static NExpect.Expectations;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestFlagExtensions
    {
        [TestFixture]
        public class GivenAnyArbStruct
        {
            public struct ArbStruct
            {
            }

            [Test]
            public void ShouldReturnFalse()
            {
                // Arrange
                ArbStruct arb;
                ArbStruct search;
                // Act
                var result = arb.HasFlag(search);
                // Assert
                Expect(result)
                    .To.Be.False();
            }
        }

        [TestFixture]
        public class ComparingEnumValues
        {
            [Test]
            public void ShouldFindExactMatch()
            {
                // Arrange
                var value = BindingFlags.Instance;
                var search = BindingFlags.Instance;
                // Act
                var result = value.HasFlag(search);
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldFindContainedFlag()
            {
                // Arrange
                var value = BindingFlags.Static | BindingFlags.NonPublic;
                var search = GetRandomFrom(new[] { BindingFlags.Static, BindingFlags.NonPublic });
                // Act
                var result = value.HasFlag(search);
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldNotFindNonPresentFlag()
            {
                // Arrange
                var value = BindingFlags.Static | BindingFlags.Public;
                var search = GetRandomFrom(new[] { BindingFlags.Instance | BindingFlags.NonPublic });
                // Act
                var result = search.HasFlag(value);
                // Assert
                Expect(result)
                    .To.Be.False();
            }
        }

        [TestFixture]
        public class ComparingIntegerValues
        {
            [Test]
            public void ShouldFindExactMatchFlag()
            {
                // Arrange
                var value = 6;
                var search = 6;
                // Act
                var result = value.HasFlag(search);
                // Assert
                Expect(result)
                    .To.Be.True();
            }
            
            [Test]
            public void ShouldFindContainedFlag2()
            {
                // Arrange
                var value = 6;
                var search = 2;
                // Act
                var result = value.HasFlag(search);
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldFindContainedFlag1()
            {
                // Arrange
                var value = 3;
                var search1 = 2;
                var search2 = 1;
                // Act
                var result1 = value.HasFlag(search1);
                var result2 = value.HasFlag(search2);
                // Assert
                Expect(result1)
                    .To.Be.True();
                Expect(result2)
                    .To.Be.True();
            }

            [Test]
            public void ShouldNotFindNonPresentFlag()
            {
                // Arrange
                var search = 1;
                var value = 4;
                // Act
                var result = value.HasFlag(search);
                // Assert
                Expect(result)
                    .To.Be.False();
            }
        }
    }
}