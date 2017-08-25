using System;
using NExpect;
using NUnit.Framework;
using static NExpect.Expectations;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestTypeEqualityTester
    {
        [TestFixture]
        public class SingleTierExactMatch
        {
            public class LeftHasId
            {
                public int Id { get; set; }
            }

            public class RightHasId
            {
                public int Id { get; set; }
            }

            [Test]
            public void AreDeepEqual_WhenHaveSameShape_ShouldCompareShape()
            {
                // Arrange
                var left = new LeftHasId() {Id = 1};
                var right = new RightHasId() {Id = 2};
                var leftType = left.GetType();
                var rightType = right.GetType();
                var sut = Create(leftType, rightType);
                // Pre-Assert
                Expect(leftType).Not.To.Equal(rightType);
                // Act
                var result = sut.AreDeepEquivalent();
                // Assert
                Expect(result).To.Be.True();
            }
        }

        [TestFixture]
        public class MultiTierExactMatch
        {
            public class LeftDog
            {
                public int Legs { get; }
            }

            public class RightDog
            {
                public int Legs { get; }
            }

            public class LeftChild
            {
                public string Name { get; }
                public LeftDog Dog { get; }
            }

            public class RightChild
            {
                public string Name { get; }
                public RightDog Dog { get; }
            }

            public class Left
            {
                public Guid Id { get; }
                public LeftChild Sub { get; }
            }

            public class Right
            {
                public Guid Id { get; }
                public RightChild Sub { get; }
            }

            [Test]
            public void DeepEquals_ShouldReturnTrue()
            {
                // Arrange
                var sut = Create(typeof(Left), typeof(Right));
                // Pre-Assert
                // Act
                var result = sut.AreDeepEquivalent();
                // Assert
                Expect(result).To.Be.True(sut.Errors.AsText("* "));
            }
        }

        [TestFixture]
        public class MultiTierMisMatch
        {
            public class LeftDog
            {
                public int Legs { get; }
                public string Name { get; }
            }

            public class RightDog
            {
                public int Legs { get; }
            }

            public class LeftChild
            {
                public string Name { get; }
                public LeftDog Dog { get; }
            }

            public class RightChild
            {
                public string Name { get; }
                public RightDog Dog { get; }
            }

            public class Left
            {
                public Guid Id { get; }
                public LeftChild Sub { get; }
            }

            public class Right
            {
                public Guid Id { get; }
                public RightChild Sub { get; }
            }

            [Test]
            public void DeepEquals_ShouldReturnTrue()
            {
                // Arrange
                var sut = Create(typeof(Left), typeof(Right));
                // Pre-Assert
                // Act
                var result = sut.AreDeepEquivalent();
                // Assert
                Expect(result).To.Be.False();
            }
        }

        [TestFixture]
        public class MultiTierSubMatch
        {
            public class LeftDog
            {
                public int Legs { get; }
            }

            public class RightDog
            {
                public int Legs { get; }
                public string Name { get; }
            }

            public class LeftChild
            {
                public string Name { get; }
                public LeftDog Dog { get; }
            }

            public class RightChild
            {
                public string Name { get; }
                public RightDog Dog { get; }
            }

            public class Left
            {
                public Guid Id { get; }
                public LeftChild Sub { get; }
            }

            public class Right
            {
                public Guid Id { get; }
                public RightChild Sub { get; }
            }

            [Test]
            public void DeepEquals_ShouldReturnTrue()
            {
                // Arrange
                var sut = Create(typeof(Left), typeof(Right));
                sut.SubMatch = true;
                // Pre-Assert
                // Act
                var result = sut.AreDeepEquivalent();
                // Assert
                Expect(result).To.Be.True(sut.Errors.AsText("* "));
            }
        }

        [TestFixture]
        public class MultiTierSubMisMatch
        {
            public class LeftDog
            {
                public int Legs { get; }
                public string Name { get; }
            }

            public class RightDog
            {
                public int Legs { get; }
            }

            public class LeftChild
            {
                public string Name { get; }
                public LeftDog Dog { get; }
            }

            public class RightChild
            {
                public string Name { get; }
                public RightDog Dog { get; }
            }

            public class Left
            {
                public Guid Id { get; }
                public LeftChild Sub { get; }
            }

            public class Right
            {
                public Guid Id { get; }
                public RightChild Sub { get; }
            }

            [Test]
            public void DeepEquals_ShouldReturnTrue()
            {
                // Arrange
                var sut = Create(typeof(Left), typeof(Right));
                sut.SubMatch = true;
                // Pre-Assert
                // Act
                var result = sut.AreDeepEquivalent();
                // Assert
                Expect(result).To.Be.False();
            }
        }

        [TestFixture]
        public class AllowingAssignmentEquivalence
        {
            [TestFixture]
            public class SingleTier
            {
                [TestFixture]
                public class SimpleUpcast
                {
                    public class Master
                    {
                        public int Id { get; }
                    }

                    public class Other
                    {
                        public long Id { get; }
                    }

                    [Test]
                    public void WhenCanUpcast_ShouldReturnTrue()
                    {
                        // Arrange
                        var sut = Create(typeof(Master), typeof(Other));
                        sut.AllowAssignmentEquivalence = true;
                        // Pre-Assert
                        // Act
                        var result = sut.AreDeepEquivalent();
                        // Assert
                        Expect(result).To.Be.True(sut.Errors.AsText("* "));
                    }
                }
            }
        }

        private static TypeEqualityTester Create(Type master, Type compare)
        {
            return new TypeEqualityTester(master, compare);
        }
    }
}