using System;
using System.Reflection;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestFlagExtensions
    {
        [TestFixture]
        public class HasFlag
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

        [TestFixture]
        public class ReadableManipulation
        {
            [Flags]
            public enum Numbers
            {
                Zero = 0,
                One = 1,
                Two = 2,
                Three = 4,
                Four = 8
            }

            [TestFixture]
            public class WithoutFlag
            {
                // this is really the reason for WithFlag - symmetry:
                // removing a flag doesn't look as intuitive, imo, so
                // let's put some wordy words on that
                [TestFixture]
                public class WhenOriginallyMissing
                {
                    [Test]
                    public void ShouldReturnTheSameValue()
                    {
                        // Arrange
                        var input = Numbers.One | Numbers.Two;
                        // Act
                        var result = input.WithoutFlag(Numbers.Three);
                        // Assert
                        Expect(result)
                            .To.Equal(input);
                    }
                }

                [TestFixture]
                public class WhenHasTheFlag
                {
                    [Test]
                    public void ShouldReturnValueWithFlagRemoved()
                    {
                        // Arrange
                        var input = Numbers.One |
                            Numbers.Two |
                            Numbers.Three |
                            Numbers.Four;

                        // Act
                        var result = input.WithoutFlag(Numbers.Three);
                        // Assert
                        Expect(
                            result.HasFlags(
                                Numbers.One,
                                Numbers.Two,
                                Numbers.Four
                            )
                        ).To.Be.True();
                        Expect(result.HasFlag(Numbers.Three))
                            .To.Be.False();
                    }
                }
            }

            [TestFixture]
            public class WithFlag
            {
                [TestFixture]
                public class WhenOriginallyMissing
                {
                    [Test]
                    public void ShouldAddTheFlag()
                    {
                        // Arrange
                        var one = Numbers.One;
                        var intOne = (int) Numbers.One;
                        var intTwo = (int) Numbers.Two;
                        var intThree = (int) Numbers.Three;
                        var intFour = (int) Numbers.Four;
                        Expect(intOne)
                            .To.Equal(1);
                        Expect(intTwo)
                            .To.Equal(2);
                        Expect(intThree)
                            .To.Equal(4);
                        Expect(intFour)
                            .To.Equal(8);
                        Expect(one.HasFlag(Numbers.Zero))
                            .To.Be.True(
                                "Zero flag has value Zero - it's like the default / Unknown enum value and is always 'included'"
                            );
                        Expect(one.HasFlag(Numbers.One))
                            .To.Be.True();
                        Expect(one.HasFlag(Numbers.Two))
                            .To.Be.False();
                        Expect(one.HasFlag(Numbers.Three))
                            .To.Be.False();
                        Expect(one.HasFlag(Numbers.Four))
                            .To.Be.False();
                        // Act
                        var oddNumbers = one.WithFlag(Numbers.Three);
                        // Assert
                        Expect(oddNumbers.HasFlag(Numbers.Zero))
                            .To.Be.True(
                                "Zero flag has value Zero - it's like the default / Unknown enum value and is always 'included'"
                            );
                        Expect(oddNumbers.HasFlag(Numbers.One))
                            .To.Be.True();
                        Expect(oddNumbers.HasFlag(Numbers.Two))
                            .To.Be.False();
                        Expect(oddNumbers.HasFlag(Numbers.Three))
                            .To.Be.True();

                        // exercise HasFlags too - might as well
                        Expect(
                            oddNumbers.HasFlags(
                                Numbers.One,
                                Numbers.Three
                            )
                        ).To.Be.True();
                        Expect(
                            oddNumbers.HasFlags(
                                Numbers.One,
                                Numbers.Three,
                                Numbers.Zero
                            )
                        ).To.Be.True("Always have Zero!");
                        Expect(
                            oddNumbers.HasFlags(
                                Numbers.One,
                                Numbers.Three,
                                Numbers.Four
                            )
                        ).To.Be.False();
                        Expect(
                            oddNumbers.HasFlags(
                                Numbers.Zero,
                                Numbers.One,
                                Numbers.Three,
                                Numbers.Two
                            )
                        ).To.Be.False();
                    }
                }

                [TestFixture]
                public class WhenOriginallyHasTheFlag
                {
                    [Test]
                    public void ShouldProvideInputValue()
                    {
                        // Arrange
                        var input = Numbers.One | Numbers.Two;
                        // Act
                        var result = input.WithFlag(Numbers.Two);
                        // Assert
                        Expect(result)
                            .To.Equal(input);
                    }
                }
            }
        }
    }
}