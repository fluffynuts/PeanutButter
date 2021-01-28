using System.Linq;
using StackExchange.Redis;
using NUnit.Framework;
using NExpect;
using static NExpect.Expectations;

namespace PeanutButter.TempRedis.Tests
{
    [TestFixture]
    public class TestTempRedis
    {
        [Test]
        public void ShouldStartUp()
        {
            // Arrange
            using var sut = new TempRedis();
            
            // Act
            var connection = ConnectionMultiplexer.Connect(
                $"localhost:{sut.Port}"
            );
            connection.GetDatabase();
            var server = connection.GetEndPoints()
                .Select(e => connection.GetServer(e))
                .FirstOrDefault();
            
            // Assert
            Expect(server)
                .Not.To.Be.Null(() => "Can't determine the redis server");
        }

        [Test]
        [Explicit("Requires downloading redis from GitHub")]
        public void ShouldBeAbleToStartFromAutoDownload()
        {
            // Arrange
            using var sut = new TempRedis(
                new TempRedisOptions()
                {
                    LocatorStrategies = RedisLocatorStrategies.DownloadForWindowsIfNecessary
                });
            // Act
            var connection = ConnectionMultiplexer.Connect(
                $"localhost:{sut.Port}"
            );
            connection.GetDatabase();
            var server = connection.GetEndPoints()
                .Select(e => connection.GetServer(e))
                .FirstOrDefault();
            // Assert
            Expect(server)
                .Not.To.Be.Null(() => "Can't determine the redis server");
        }
    }
}