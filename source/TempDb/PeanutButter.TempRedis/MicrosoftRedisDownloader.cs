using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PeanutButter.Utils;

namespace PeanutButter.TempRedis
{
    /// <summary>
    /// Attempts to download the Microsoft-built redis server 3.2.100
    /// from https://github.com/microsoftarchive/redis/releases/tag/win-3.2.100
    /// which is adequate for testing on a windows machine
    /// </summary>
    public interface IRedisDownloader
    {
        /// <summary>
        /// Attempts to fetch the 3.2.100 release of redis from
        /// microsoft archives on github &amp; return the path to the
        /// downloaded, extracted redis-server.exe
        /// </summary>
        /// <returns></returns>
        Task<string> Fetch();
    }

    /// <inheritdoc />
    public class MicrosoftRedisDownloader
        : IRedisDownloader
    {
        private const string RELEASE_URL =
            "https://github.com/microsoftarchive/redis/releases/download/win-3.2.100/Redis-x64-3.2.100.zip";

        /// <inheritdoc />
        public async Task<string> Fetch()
        {
            var existing = RedisServerExe;
            if (File.Exists(existing))
            {
                return existing;
            }

            await FetchZipAndDecompress();
            return FindRedisServerExecutable();
        }

        private string RedisServerExe => Path.Combine(WorkingFolder, "redis-server.exe");

        private string FindRedisServerExecutable()
        {
            var seek = RedisServerExe;
            if (!File.Exists(seek))
            {
                throw new DownloadFailedException(
                    $"Unpacked archive doesn't contain a redis-server.exe binary"
                );
            }
            return seek;
        }

        private async Task FetchZipAndDecompress()
        {
            try
            {
                var req = WebRequest.Create(RELEASE_URL);
                using var res = await req.GetResponseAsync();
                using var stream = res.GetResponseStream();
                if (stream is null)
                {
                    throw new DownloadFailedException($"Unable to read stream from release url {RELEASE_URL}");
                }

                using var archive = new ZipArchive(stream);
                await ExpandArchive(archive);
            }
            catch (DownloadFailedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DownloadFailedException($"Unable to download win32 redis from {RELEASE_URL}: {ex.Message}",
                    ex);
            }
        }

        private async Task ExpandArchive(ZipArchive archive)
        {
            foreach (var entry in archive.Entries)
            {
                var stream = entry.Open();
                var target = Path.Combine(WorkingFolder, entry.FullName);
                await stream.SaveAsync(target);
            }
        }

        /// <summary>
        /// Where Redis will be extracted to, if all things go as planned
        /// </summary>
        public string WorkingFolder =>
            _workDir ??= DetermineExtractionFolder();

        private string _workDir;

        private string DetermineExtractionFolder()
        {
            return Environment.GetEnvironmentVariable("TEMP_REDIS_WORKDIR")
                ?? Path.Combine(ContainingFolder(), "redis-server");
        }

        private string ContainingFolder()
        {
            var asm = typeof(MicrosoftRedisDownloader).Assembly;
            var asmFile = new Uri(asm.Location).LocalPath;
            return Path.GetDirectoryName(asmFile);
        }
    }

    internal class DownloadFailedException : Exception
    {
        internal DownloadFailedException(
            string message,
            Exception innerException = null
        ) : base(message, innerException)
        {
        }
    }
}