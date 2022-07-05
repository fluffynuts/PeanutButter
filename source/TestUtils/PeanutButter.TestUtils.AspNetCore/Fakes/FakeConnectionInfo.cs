using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PeanutButter.TestUtils.AspNetCore.Fakes
{
    public class FakeConnectionInfo : ConnectionInfo
    {
        public override Task<X509Certificate2> GetClientCertificateAsync(
            CancellationToken cancellationToken = new()
        )
        {
            return Task.FromResult(ClientCertificate);
        }

        public override string Id { get; set; }
        public override IPAddress RemoteIpAddress { get; set; }
        public override int RemotePort { get; set; }
        public override IPAddress LocalIpAddress { get; set; }
        public override int LocalPort { get; set; }
        public override X509Certificate2 ClientCertificate { get; set; }
    }
}