using NExpect;
using NExpect.Implementations;
using NExpect.Interfaces;
using NExpect.MatcherLogic;
using NUnit.Framework;
using PeanutButter.SimpleTcpServer;
using static NExpect.Expectations;

namespace PeanutButter.SimpleHTTPServer.Tests;

[TestFixture]
public class TestHttpServerFactory
{
    [Test]
    public void ShouldProvideAndReUseServers()
    {
        // Arrange
        // Act
        HttpServer server1;
        HttpServer server2;
        HttpServer server3;

        using (var sut = Create())
        {
            using (var lease1 = sut.BorrowServer())
            {
                server1 = lease1.Item;
                using var lease2 = sut.BorrowServer();
                server2 = lease2.Item;
            }

            using (var lease3 = sut.BorrowServer())
            {
                server3 = lease3.Item;
            }

            // Assert
            Expect(server1)
                .To.Be.An.Instance.Of<HttpServer>()
                .And
                .Not.To.Be(server2);
            Expect(server2)
                .To.Be.An.Instance.Of<HttpServer>()
                .And
                .To.Be(server3);
        }

        Expect(server1)
            .To.Be.Disposed();
        Expect(server2)
            .To.Be.Disposed();
        Expect(server3)
            .To.Be.Disposed();
    }

    private static HttpServerFactory Create()
    {
        return new HttpServerFactory();
    }
}

public static class TrackingDisposablesMatchers
{
    public static IMore<T> Disposed<T>(
        this IBe<T> be
    ) where T : ITrackingDisposable
    {
        return be.AddMatcher(
            actual =>
            {
                var passed = actual.Disposed;
                return new MatcherResult(
                    passed,
                    () => $"Expected {actual} {passed.AsNot()}to have been disposed"
                );
            }
        );
    }
}