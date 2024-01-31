using System.Linq;
using NSubstitute;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.Generic.Tests;

[TestFixture]
public class TestSubstituteExtensions
{
    [Test]
    public void ShouldBeAbleToPullArgumentsPerCallInvocationByMethodName()
    {
        // Arrange
        var sub = Substitute.For<ICalculator>()
            .With(
                o => o.Add(Arg.Any<int>(), Arg.Any<int>())
                    .Returns(
                        ci =>
                        {
                            var a = (int)ci.Args()[0];
                            var b = (int)ci.Args()[1];
                            return a + b;
                        }
                    )
            );
        // Act
        sub.Add(1, 2);
        sub.Add(101, 202);
        sub.Add(13, 14);

        // Assert
        var allCalls = sub.ReceivedCalls(nameof(ICalculator.Add));
        Expect(allCalls)
            .To.Contain.Only(3).Items();
        Expect(allCalls[0].Arguments)
            .To.Equal(
                new[]
                {
                    1,
                    2
                }.Cast<object>()
            );
        Expect(allCalls[1].Arguments)
            .To.Equal(
                new[]
                {
                    101,
                    202
                }.Cast<object>()
            );
        Expect(allCalls[2].Arguments)
            .To.Equal(
                new[]
                {
                    13,
                    14
                }.Cast<object>()
            );
    }

    [Test]
    public void ShouldBeAbleToPullArgumentsByMatcher()
    {
        // Arrange
        var c1 = new Calculator1();
        var c2 = new Calculator2();
        var cupboard = Substitute.For<ICupboard>();
        // Act
        cupboard.Stow(c1);
        cupboard.Stow(c2);
        // Assert
        var call2 = cupboard.ReceivedCalls(
            c => c.MethodName == "Stow" && c.Arguments[0] is Calculator2
        ).Single();
        var call1 = cupboard.ReceivedCalls(
            c => c.MethodName == "Stow" && c.Arguments[0] is Calculator1
        ).Single();
        Expect(call2.Arguments)
            .To.Equal(new[] { c2 });
        Expect(call1.Arguments)
            .To.Equal(new[] { c1 });
    }

    public interface ICalculator
    {
        int Add(int a, int b);
    }

    public interface ICupboard
    {
        void Stow(ICalculator calculator);
    }

    public class Calculator1 : ICalculator
    {
        public int Add(int a, int b)
        {
            return a + b;
        }
    }

    public class Calculator2 : ICalculator
    {
        public int Add(int a, int b)
        {
            return a + b;
        }
    }
}