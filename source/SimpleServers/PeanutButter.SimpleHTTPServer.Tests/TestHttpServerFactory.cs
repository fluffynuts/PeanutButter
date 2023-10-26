using NExpect;
using NUnit.Framework;
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
        IHttpServer server1;
        IHttpServer server2;
        IHttpServer server3;

        using (var sut = Create())
        {
            using (var lease1 = sut.Borrow())
            {
                server1 = lease1.Instance;
                using var lease2 = sut.Borrow();
                server2 = lease2.Instance;
            }

            using (var lease3 = sut.Borrow())
            {
                server3 = lease3.Instance;
            }

            // Assert
            Expect(server1)
                .To.Be.An.Instance.Of<HttpServer>()
                .And
                .Not.To.Be(server2);
            Expect(server3)
                .To.Be.An.Instance.Of<HttpServer>()
                .And
                .To.Be(server1);
        }

        Expect(server1.Disposed)
            .To.Be.True();
        Expect(server2.Disposed)
            .To.Be.True();
        Expect(server3.Disposed)
            .To.Be.True();
    }

    private static HttpServerFactory Create()
    {
        return new HttpServerFactory();
    }
}