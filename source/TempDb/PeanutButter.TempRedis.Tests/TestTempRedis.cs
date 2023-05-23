using System;
using System.Linq;
using System.Threading;
using StackExchange.Redis;
using NUnit.Framework;
using NExpect;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

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
                $"127.0.0.1:{sut.Port}"
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
        public void ShouldProvideConvenienceConnectMethod()
        {
            // Arrange
            using var sut = new TempRedis();
            
            // Act
            var connection = sut.Connect();
            connection.GetDatabase();
            var server = connection.GetEndPoints()
                .Select(e => connection.GetServer(e))
                .FirstOrDefault();
            
            // Assert
            Expect(server)
                .Not.To.Be.Null(() => "Can't determine the redis server");
        }

        [Test]
        [Timeout(10000)]
        public void ShouldRestartOnAccidentalDeath()
        {
            // Arrange
            using var sut = new TempRedis();
            var key = GetRandomString();
            var value = GetRandomString();
            // Act
            Console.Error.WriteLine("Attempt to connect");
            var connection = ConnectionMultiplexer.Connect(
                $"127.0.0.1:{sut.Port}",
                o =>
                {
                    o.ReconnectRetryPolicy = new LinearRetry(250);
                    o.AbortOnConnectFail = false;
                    o.ConnectRetry = 10;
                    o.ConnectTimeout = 500;
                    o.AsyncTimeout = 1000;
                    o.SyncTimeout = 1000;
                }
            );
            var db = connection.GetDatabase();
            db.StringSet(key, value);
            sut.ServerProcess.Kill();
            do
            {
                Thread.Sleep(1);
            } while (sut.ServerProcess is null || !sut.ServerProcess.HasExited);
            var result = (string)db.StringGet(key);
            
            // Assert
            Expect(result)
                .To.Equal(value);
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
                $"127.0.0.1:{sut.Port}"
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