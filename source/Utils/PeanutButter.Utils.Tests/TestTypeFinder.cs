using System;
using NUnit.Framework;
using NExpect;
using PeanutButter.Utils.Tests.TypeFinderTypes;
using static NExpect.Expectations;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestTypeFinder
    {
        [TestFixture]
        public class WhenProvidedAssemblies
        {
            [Test]
            public void ShouldLoadTypeByShortName()
            {
                // Arrange
                // Act
                var result = TypeFinder.TryFind(
                    nameof(MooCowBeefCake),
                    typeof(TestTypeFinder).Assembly
                );
                // Assert
                Expect(result)
                    .To.Be(typeof(MooCowBeefCake));
            }

            [Test]
            public void ShouldLoadTypeByShortNameWithStringComparison()
            {
                // Arrange
                // Act
                var result = TypeFinder.TryFind(
                    nameof(MooCowBeefCake).ToLower(),
                    StringComparison.OrdinalIgnoreCase,
                    typeof(TestTypeFinder).Assembly,
                    typeof(SingleItemCache<>).Assembly
                );
                // Assert
                Expect(result)
                    .To.Be(typeof(MooCowBeefCake));
            }

            [Test]
            public void ShouldLoadTypeByFullName1()
            {
                // Arrange
                // Act
                var result = TypeFinder.TryFind(
                    "PeanutButter.Utils.Tests.TypeFinderTypes.MooCowBeefCake",
                    typeof(TestTypeFinder).Assembly
                );
                // Assert
                Expect(result)
                    .To.Be(typeof(MooCowBeefCake));
            }

            [Test]
            public void ShouldLoadTypeByFullName2()
            {
                // Arrange
                // Act
                var result = TypeFinder.TryFind(
                    typeof(MooCowBeefCake).FullName,
                    typeof(TestTypeFinder).Assembly
                );
                // Assert
                Expect(result)
                    .To.Be(typeof(MooCowBeefCake));
            }

            [Test]
            public void ShouldLoadTypeByFullNameWithStringComparison()
            {
                // Arrange
                // Act
                var result = TypeFinder.TryFind(
                    "PeanutButter.Utils.Tests.TypeFinderTypes.moocowbeefcake",
                    StringComparison.OrdinalIgnoreCase,
                    typeof(TestTypeFinder).Assembly
                );
                // Assert
                Expect(result)
                    .To.Be(typeof(MooCowBeefCake));
            }
        }

        [TestFixture]
        public class WhenProvidedNoAssemblies
        {
            [Test]
            public void ShouldLoadTypeByShortName()
            {
                // Arrange
                // Act
                var result = TypeFinder.TryFind(
                    nameof(MooCowBeefCake)
                );
                // Assert
                Expect(result)
                    .To.Be(typeof(MooCowBeefCake));
            }

            [Test]
            public void ShouldLoadTypeByShortNameWithStringComparison()
            {
                // Arrange
                // Act
                var result = TypeFinder.TryFind(
                    nameof(ExtensionsForIEnumerables).ToLower(),
                    StringComparison.OrdinalIgnoreCase
                );
                // Assert
                Expect(result)
                    .To.Be(typeof(ExtensionsForIEnumerables));
            }

            [Test]
            public void ShouldLoadTypeByFullName1()
            {
                // Arrange
                // Act
                var result = TypeFinder.TryFind(
                    "NExpect.Expectations"
                );
                // Assert
                Expect(result)
                    .To.Be(typeof(Expectations));
            }

            [Test]
            public void ShouldLoadTypeByFullName2()
            {
                // Arrange
                var t = typeof(Expectations);
                var constructed = $"{t.Namespace}.{t.Name}";
                Expect(constructed)
                    .To.Equal(t.FullName);
                // Act
                var result = TypeFinder.TryFind(
                    typeof(Expectations).FullName
                );
                // Assert
                Expect(result)
                    .To.Be(typeof(Expectations));
            }

            [Test]
            public void ShouldLoadTypeByFullNameWithStringComparison()
            {
                // Arrange
                // Act
                var result = TypeFinder.TryFind(
                    "expectations",
                    StringComparison.OrdinalIgnoreCase
                );
                // Assert
                Expect(result)
                    .To.Be(typeof(Expectations));
            }
        }
    }
}

namespace PeanutButter.Utils.Tests.TypeFinderTypes
{
    public class MooCowBeefCake
    {
        public int Id { get; }
        public string Name { get; }
    }
}