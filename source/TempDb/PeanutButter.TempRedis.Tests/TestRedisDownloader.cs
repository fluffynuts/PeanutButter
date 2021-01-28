using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NExpect;
using NUnit.Framework;
using PeanutButter.Utils;
using static NExpect.Expectations;

namespace PeanutButter.TempRedis.Tests
{
    [TestFixture]
    public class TestRedisDownloader
    {
        [Test]
        [Explicit("Downloads an archive from github")]
        public async Task ShouldDownloadAndReturnPathToRedisServerExe()
        {
            // Arrange
            var sut = Create();
            // Act
            var redisPath = await sut.Fetch();
            // Assert
            Expect(redisPath)
                .To.Exist();
        }

        [Test]
        [Explicit("Downloads an archive from github")]
        public async Task ShouldReUseExistingDownload()
        {
            // Arrange
            using var tempFolder = new AutoTempFolder();
            Environment.SetEnvironmentVariable("TEMP_REDIS_WORKDIR", tempFolder.Path);
            var sut = Create();
            // Act
            var first = await sut.Fetch();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var second = await sut.Fetch();
            stopwatch.Stop();
            // Assert
            Expect(stopwatch.ElapsedMilliseconds)
                .To.Be.Less.Than(100);
            Expect(first)
                .To.Equal(second);
        }

        private MicrosoftRedisDownloader Create()
        {
            return new MicrosoftRedisDownloader();
        }
    }
}