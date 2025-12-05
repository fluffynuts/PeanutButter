using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace PeanutButter.Utils.Tests;

[TestFixture]
public class TestStepContext
{
    [TestFixture]
    public class OperatingOnAction
    {
        [Test]
        public void ShouldRunBefore_Action_After()
        {
            // Arrange
            var sut = Create();
            var collected = new List<string>();
            // Act
            sut.Run(
                () => collected.Add("before"),
                () => collected.Add("action"),
                _ => collected.Add("after")
            );
            // Assert
            Expect(collected)
                .To.Equal(
                    ["before", "action", "after"]
                );
        }

        [Test]
        public void ShouldThrowWhenBeforeThrows()
        {
            // Arrange
            var sut = Create();
            var exception = new InvalidOperationException("nope");
            Exception captured = null;
            // Act
            Expect(
                    () => sut.Run(
                        () => throw exception,
                        () => throw new Exception("shouldn't get here"),
                        e => captured = e
                    )
                ).To.Throw<InvalidOperationException>()
                .With.Message.Equal.To("nope");
            // Assert
            Expect(captured)
                .To.Be(null);
        }

        [Test]
        public void ShouldPassExceptionFromMiddleIntoAfter()
        {
            // Arrange
            var sut = Create();
            var exception = new InvalidOperationException("nope");
            Exception captured = null;
            // Act
            sut.Run(
                () =>
                {
                },
                () => throw exception,
                e => captured = e
            );
            // Assert
            Expect(captured)
                .To.Be(exception);
        }

        [Test]
        public void ShouldRethroExceptionFromAfter()
        {
            // Arrange
            var sut = Create();
            // Act
            Expect(
                    () =>
                        sut.Run(
                            () =>
                            {
                            },
                            () =>
                            {
                            },
                            _ => throw new AuthenticationException("denied")
                        )
                ).To.Throw<AuthenticationException>()
                .With.Message.Containing("denied");
            // Assert
        }
    }

    [TestFixture]
    public class OperatingOnAsyncAction
    {
        [Test]
        public async Task ShouldRunBefore_Action_After()
        {
            // Arrange
            var sut = Create();
            var collected = new List<string>();
            // Act
            await sut.RunAsync(
                async () => await Task.Run(() => collected.Add("before")),
                async () => await Task.Run(() => collected.Add("action")),
                async (_) => await Task.Run(() => collected.Add("after"))
            );
            // Assert
            Expect(collected)
                .To.Equal(
                    ["before", "action", "after"]
                );
        }

        [Test]
        public void ShouldThrowWhenBeforeThrows()
        {
            // Arrange
            var sut = Create();
            var exception = new InvalidOperationException("nope");
            Exception captured = null;
            // Act
            Expect(
                    async () => await sut.RunAsync(
                        async () => await Task.Run(() => throw exception),
                        async () => await Task.Run(() => throw new Exception("shouldn't get here")),
                        async e => await Task.Run(() => captured = e)
                    )
                ).To.Throw<InvalidOperationException>()
                .With.Message.Equal.To("nope");
            // Assert
            Expect(captured)
                .To.Be(null);
        }

        [Test]
        public async Task ShouldPassExceptionFromMiddleIntoAfter()
        {
            // Arrange
            var sut = Create();
            var exception = new InvalidOperationException("nope");
            Exception captured = null;
            // Act
            await sut.RunAsync(
                () => Task.CompletedTask,
                async () => await Task.Run(() => throw exception),
                async e =>
                {
                    await Task.Run(() => captured = e);
                    return ErrorHandlerResult.Suppress;
                }
            );
            // Assert
            Expect(captured)
                .To.Be(exception);
        }

        [Test]
        public void ShouldRethrowExceptionFromAfter()
        {
            // Arrange
            var sut = Create();
            // Act
            Expect(
                    async () =>
                        await sut.RunAsync(
                            () => Task.CompletedTask,
                            () => Task.CompletedTask,
                            async _ => await Task.Run(() => throw new AuthenticationException("denied"))
                        )
                ).To.Throw<AuthenticationException>()
                .With.Message.Containing("denied");
            // Assert
        }
    }

    private static Steps Create()
    {
        return new Steps();
    }
}