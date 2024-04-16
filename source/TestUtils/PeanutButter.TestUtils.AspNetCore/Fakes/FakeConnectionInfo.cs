using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
// ReSharper disable ClassNeverInstantiated.Global

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore.Fakes;
#else
namespace PeanutButter.TestUtils.AspNetCore.Fakes;
#endif

/// <summary>
/// Provides a fake connection
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class FakeConnectionInfo : ConnectionInfo, IFake
{
    /// <inheritdoc />
    public override Task<X509Certificate2> GetClientCertificateAsync(
        CancellationToken cancellationToken = new()
    )
    {
        return Task.FromResult(ClientCertificate);
    }

    /// <inheritdoc />
    public override string Id { get; set; } = Guid.NewGuid().ToString();

    /// <inheritdoc />
    public override IPAddress RemoteIpAddress { get; set; } = IPAddress.Parse(LOCALHOST);

    /// <inheritdoc />
    public override int RemotePort { get; set; } = 8080;

    /// <inheritdoc />
    public override IPAddress LocalIpAddress { get; set; } = IPAddress.Parse(LOCALHOST);

    /// <inheritdoc />
    public override int LocalPort { get; set; } = 80;

    /// <inheritdoc />
    public override X509Certificate2 ClientCertificate { get; set; }

    private const string LOCALHOST = "127.0.0.1";
}