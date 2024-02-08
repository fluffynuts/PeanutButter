using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using PeanutButter.TestUtils.AspNetCore.Builders;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.AspNetCore.Tests;

[TestFixture]
public class TestWebSocketBuilder
{
    [TestFixture]
    public class DefaultBuild
    {
        [Test]
        public void ShouldBeAbleToAbort()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(() => result.Abort())
                .Not.To.Throw();
            Expect(result.State)
                .To.Equal(WebSocketState.Aborted);
        }

        [Test]
        public void ShouldNotBeAborted()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.State)
                .To.Equal(WebSocketState.None);
        }

        [Test]
        public void ShouldBeAbleToClose()
        {
            // Arrange
            var status = GetRandom<WebSocketCloseStatus>();
            var description = GetRandomString();
            var token = new CancellationToken();
            // Act
            var result = BuildDefault();
            Expect(async () => await result.CloseAsync(
                status, description, token
            )).Not.To.Throw();

            // Assert
            Expect(result.CloseStatus)
                .To.Equal(status);
            Expect(result.CloseStatusDescription)
                .To.Equal(description);
            Expect(result.State)
                .To.Equal(WebSocketState.Closed);
        }

        [Test]
        public void ShouldBeAbleToCloseOutput()
        {
            // Arrange
            var status = GetRandom<WebSocketCloseStatus>();
            var description = GetRandomString();
            var token = new CancellationToken();
            // Act
            var result = BuildDefault();
            Expect(async () => await result.CloseOutputAsync(
                status, description, token
            )).Not.To.Throw();

            // Assert
            Expect(result.CloseStatus)
                .To.Equal(status);
            Expect(result.CloseStatusDescription)
                .To.Equal(description);
            Expect(result.State)
                .To.Equal(WebSocketState.Closed);
        }

        [Test]
        public void ShouldCloseWhenDisposed()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            result.Dispose();
            // Assert
            Expect(result.State)
                .To.Equal(WebSocketState.Closed);
        }

        [Test]
        public void ShouldReceiveWithoutError()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            Expect(async () => await result.ReceiveAsync(
                GetRandomBytes().ToArraySegment(),
                CancellationToken.None)
            ).Not.To.Throw();
            // Assert
        }

        [Test]
        public void ShouldSendWithoutError()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(async () => await result.SendAsync(
                GetRandomBytes().ToArraySegment(),
                WebSocketMessageType.Binary,
                true,
                CancellationToken.None)
            ).Not.To.Throw();
        }

        private static WebSocket BuildDefault()
        {
            return WebSocketBuilder.BuildDefault();
        }
    }

    [Test]
    public async Task ShouldBeAbleToInstallReceiveHandler()
    {
        // Arrange
        var expected = GetRandom<WebSocketReceiveResult>();
        var capturedBuffer = null as ArraySegment<byte>?;
        var capturedToken = null as CancellationToken?;
        var inputBuffer = GetRandomBytes().ToArraySegment();
        var token = new CancellationToken();
        // Act
        var result = WebSocketBuilder.Create()
            .WithReceiveHandler(
                (buffer, cancellationToken) =>
                {
                    capturedBuffer = buffer;
                    capturedToken = cancellationToken;
                    return Task.FromResult(expected);
                }
            ).Build();
        var response = await result.ReceiveAsync(inputBuffer, token);
        // Assert
        Expect(response)
            .To.Be(expected);
        Expect(capturedBuffer)
            .To.Equal(inputBuffer);
        Expect(capturedToken)
            .To.Equal(token);
    }

    [Test]
    public void ShouldBeAbleToInstallSendHandler()
    {
        // Arrange
        var expectedTask = Task.CompletedTask;
        var inputBuffer = GetRandomBytes().ToArraySegment();
        var inputType = GetRandom<WebSocketMessageType>();
        var inputComplete = GetRandomBoolean();
        var inputToken = new CancellationToken();
        var capturedBuffer = null as ArraySegment<byte>?;
        var capturedType = null as WebSocketMessageType?;
        var capturedComplete = null as bool?;
        var capturedToken = null as CancellationToken?;
        // Act
        var result = WebSocketBuilder.Create()
            .WithSendHandler(
                (buffer, type, complete, token) =>
                {
                    capturedBuffer = buffer;
                    capturedType = type;
                    capturedComplete = complete;
                    capturedToken = token;
                    return expectedTask;
                }
            ).Build();
        var response = result.SendAsync(
            inputBuffer,
            inputType,
            inputComplete,
            inputToken
        );
        // Assert
        Expect(response)
            .To.Be(expectedTask);
        Expect(capturedBuffer)
            .To.Equal(inputBuffer);
        Expect(capturedType)
            .To.Equal(inputType);
        Expect(capturedComplete)
            .To.Equal(inputComplete);
        Expect(capturedToken)
            .To.Equal(inputToken);
    }

    [Test]
    public void ShouldBeAbleToSetSubProtocol()
    {
        // Arrange
        var expected = GetRandomString();
        // Act
        var result = WebSocketBuilder.Create()
            .WithSubProtocol(expected)
            .Build();
        // Assert
        Expect(result.SubProtocol)
            .To.Equal(expected);
    }
}