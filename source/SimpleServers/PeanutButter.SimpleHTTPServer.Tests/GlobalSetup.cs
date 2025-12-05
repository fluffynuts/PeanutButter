using PeanutButter.Utils;

namespace PeanutButter.SimpleHTTPServer.Tests;

[SetUpFixture]
public class GlobalSetup
{
    public static Pool<HttpServer> Pool { get; private set; }

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        Pool = new Pool<HttpServer>(
            () => new HttpServer(),
            s => s.Reset()
        );
    }

    [OneTimeTearDown]
    public void OneTimeTeardown()
    {
        Pool?.Dispose();
        Pool = null;
    }
}