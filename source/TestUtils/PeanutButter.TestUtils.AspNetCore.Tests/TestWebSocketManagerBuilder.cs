using System.Net.WebSockets;
using System.Threading.Tasks;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using NSubstitute;
using PeanutButter.TestUtils.AspNetCore.Builders;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using static NExpect.Expectations;

namespace PeanutButter.TestUtils.AspNetCore.Tests;

[TestFixture]
public class TestWebSocketManagerBuilder
{
    [TestFixture]
    public class DefaultBuild
    {
        [Test]
        public async Task ShouldAcceptWebSocket()
        {
            // Arrange
            var sut = BuildDefault();
            // Act
            var result = await sut.AcceptWebSocketAsync(GetRandomString());
            // Assert
            Expect(result)
                .Not.To.Be.Null()
                .And
                .To.Be.An.Instance.Of<WebSocket>();
        }

        [Test]
        public void ShouldNotBeAWebSocketRequest()
        {
            // Arrange
            var sut = BuildDefault();
            // Act
            var result = sut.IsWebSocketRequest;
            // Assert
            Expect(result)
                .To.Be.False();
        }

        [Test]
        public void ShouldHaveEmptyRequestedProtocols()
        {
            // Arrange
            var sut = BuildDefault();
            // Act
            var result = sut.WebSocketRequestedProtocols;
            // Assert
            Expect(result)
                .To.Be.Empty();
        }

        private static FakeWebSocketManager BuildDefault()
        {
            return WebSocketManagerBuilder.BuildDefault() as FakeWebSocketManager;
        }
    }

    [Test]
    public async Task ShouldAllowSettingAcceptHandler()
    {
        // Arrange
        var expected = Substitute.For<WebSocket>();
        var captured = null as string;
        var id = GetRandomString();
        // Act
        var sut = WebSocketManagerBuilder.Create()
            .WithWebSocketAcceptHandler(s =>
            {
                captured = s;
                return Task.FromResult(expected);
            }).Build();
        var result = await sut.AcceptWebSocketAsync(id);
        // Assert
        Expect(captured)
            .To.Equal(id);
        Expect(result)
            .To.Be(expected);
    }

    [Test]
    public void ShouldAllowSettingIsWebSocketRequest()
    {
        // Arrange
        // Act
        var result = WebSocketManagerBuilder.Create()
            .WithIsWebSocketRequest(true)
            .Build();
        // Assert
        Expect(result.IsWebSocketRequest)
            .To.Be.True();
    }

    [Test]
    public void ShouldAllowSettingRequestProtocols()
    {
        // Arrange
        var expected = GetRandomArray<string>(1);
        // Act
        var result = WebSocketManagerBuilder.Create()
            .WithWebSocketRequestedProtocols(expected)
            .Build();
        // Assert
        Expect(result.WebSocketRequestedProtocols)
            .To.Equal(expected);
    }
}