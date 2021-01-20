using System.Linq;
using StackExchange.Redis;
using NUnit.Framework;
using NExpect;
using static NExpect.Expectations;

namespace PeanutButter.TempRedis.Tests
{
    [TestFixture]
    public class TempRedisTests
    {
        [Test]
        public void ShouldStartUp()
        {
            using var tempRedis = new TempRedis();
            var connection = ConnectionMultiplexer.Connect(
                $"localhost:{tempRedis.Port}"
            );
            connection.GetDatabase();
            var server = connection.GetEndPoints()
                .Select(e => connection.GetServer(e))
                .FirstOrDefault();
            Expect(server)
                .Not.To.Be.Null(() => "Can't determine the redis server");
        }
    }
}