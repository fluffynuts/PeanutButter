using NSubstitute;
using static PeanutButter.Utils.FlexiConstructor;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Utils.Tests;

[TestFixture]
public class TestFlexiConstructor
{
    [TestFixture]
    public class WhenNoConstructorArgs
    {
        [Test]
        public void ShouldReturnNewItemEveryTime()
        {
            // Arrange
            // Act
            var result1 = Construct<NoDependencies>();
            var result2 = Construct<NoDependencies>();
            // Assert
            Expect(result1)
                .To.Be.An.Instance.Of<NoDependencies>();
            Expect(result2)
                .To.Be.An.Instance.Of<NoDependencies>();
            Expect(result1)
                .Not.To.Be(result2);
        }

        public class NoDependencies;
    }

    [TestFixture]
    public class WhenSingleIntParameter
    {
        [Test]
        public void ShouldConstructByDefaultWithZero()
        {
            // Arrange
            // Act
            var result = Construct<Container<int>>();
            // Assert
            Expect(result)
                .Not.To.Be.Null();
            Expect(result.Value)
                .To.Equal(0);
        }

        [Test]
        public void ShouldPassInTheProvidedInt()
        {
            // Arrange
            var expected = GetRandomInt(1);
            // Act
            var result = Construct<Container<int>>(expected);
            // Assert
            Expect(result.Value)
                .To.Equal(expected);
        }

        public class Container<T>
        {
            public T Value { get; }

            public Container(T value)
            {
                Value = value;
            }
        }
    }

    [TestFixture]
    public class WhenMultipleParametersOfSameType
    {
        [Test]
        public void ShouldKeepThemInOrder()
        {
            // Arrange
            var expected1 = GetRandomInt(35, 50);
            var expected2 = GetRandomInt(5, 25);
            // Act
            var result = Construct<Container<int>>(expected1, expected2);
            // Assert
            Expect(result.Value1)
                .To.Equal(expected1);
            Expect(result.Value2)
                .To.Equal(expected2);
        }

        [Test]
        public void ShouldDiscardUselessParameters()
        {
            // Arrange
            var first = GetRandomInt(35, 50);
            var second = GetRandomInt(10, 20);
            // Act
            var result = Construct<Container<int>>(first, "a", true, second, new Container<int>(4, 5));
            // Assert
            Expect(result.Value1)
                .To.Equal(first);
            Expect(result.Value2)
                .To.Equal(second);
        }

        public class Container<T>(T value1, T value2)
        {
            public T Value1 { get; } = value1;
            public T Value2 { get; } = value2;
        }

        [Test]
        public void ShouldProvideOutOfOrderServiceDependencies()
        {
            // Arrange
            var router = Substitute.For<IRouter>();
            var caller = Substitute.For<ICaller>();
            var wibbles = Substitute.For<IWibbles>();
            var otherCaller = Substitute.For<ICaller>();
            // Act
            var result1 = Construct<HairyService>(router, caller, wibbles, otherCaller);
            var result2 = Construct<HairyService>(caller, wibbles, router, otherCaller);
            var result3 = Construct<HairyService>(caller, router, wibbles, otherCaller);
            // Assert
            foreach (var item in new[]
                     {
                         result1,
                         result2,
                         result3
                     })
            {
                Expect(item)
                    .Not.To.Be.Null();
                Expect(item.Router)
                    .To.Be(router);
                Expect(item.Caller)
                    .To.Be(caller);
                Expect(item.Wibbles)
                    .To.Be(wibbles);
            }
        }

        [TestFixture]
        public class Validation
        {
            [Test]
            public void ShouldValidateExactTypesOnDemand()
            {
                // Arrange
                var router = new DerivedRouter();
                var caller = Substitute.For<ICaller>();
                var wibbles = Substitute.For<IWibbles>();

                // Act
                Expect(
                        () => Construct<HairyService>(
                            ConstructFlags.MatchTypesExactly,
                            caller,
                            router,
                            wibbles
                        )
                    )
                    .To.Throw<InvalidOperationException>()
                    .With.Message.Containing(nameof(IRouter))
                    .And.Containing(nameof(DerivedRouter));
                // Assert
            }

            [Test]
            public void ShouldValidateNoUnusedArgumentsOnDemand()
            {
                // Arrange
                // Act
                Expect(
                    () => Construct<SimpleService>(
                        ConstructFlags.ErrorWhenUnusedArguments,
                        Substitute.For<ICaller>(),
                        Substitute.For<IRouter>()
                    )
                ).To.Throw<InvalidOperationException>();
                // Assert
            }

            [Test]
            public void ShouldValidateNoUnspecifiedParametersOnDemand()
            {
                // Arrange
                Expect(
                    () =>
                        Construct<HairyService>(
                            ConstructFlags.ErrorWhenUnspecifiedParameters,
                            Substitute.For<IRouter>()
                        )
                ).To.Throw<InvalidOperationException>();

                // Act
                // Assert
            }
        }

        public class DerivedRouter : IDerivedRouter;

        public interface IRouter;

        public interface IDerivedRouter : IRouter;

        public interface ICaller;

        public interface IWibbles;

        public class HairyService(
            IRouter router,
            ICaller caller,
            IWibbles wibbles
        )
        {
            public IRouter Router { get; } = router;
            public ICaller Caller { get; } = caller;
            public IWibbles Wibbles { get; } = wibbles;
        }

        public class SimpleService(
            IRouter router
        )
        {
            public IRouter Router { get; } = router;
        }
    }
}