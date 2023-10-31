using System;
using System.Collections.Concurrent;
using System.IO;
using PeanutButter.Utils;

namespace PeanutButter.TempRedis
{
    /// <summary>
    /// Attempts to find a redis-server executable for TempRedis
    /// </summary>
    public static class RedisExecutableFinder
    {
        /// <summary>
        /// Attempts to find a redis executable according to the provided
        /// strategies (flags)
        /// - in path
        /// - as a windows service (win32 only)
        /// - download a windows executable from https://github.com/microsoftarchive/redis/releases/download/win-3.2.100/Redis-x64-3.2.100.zip
        /// </summary>
        /// <param name="locatorStrategies"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static string FindRedisExecutable(RedisLocatorStrategies locatorStrategies)
        {
            var result = CachedRedisServerLocations.FindOrAdd(
                locatorStrategies,
                () => FindRedisExecutableOnce(locatorStrategies)
            );
            if (File.Exists(result))
            {
                return result;
            }
            CachedRedisServerLocations.TryRemove(locatorStrategies, out _);
            return FindRedisExecutable(locatorStrategies);
        }

        private static readonly ConcurrentDictionary<RedisLocatorStrategies, string> CachedRedisServerLocations = new();

        private static string FindRedisExecutableOnce(RedisLocatorStrategies locatorStrategies)
        {
            var lookInPath = locatorStrategies.HasFlag(RedisLocatorStrategies.FindInPath);
            if (lookInPath)
            {
                // prefer redis-server in the path
                var inPath = Find.InPath("redis-server");
                if (inPath is not null)
                {
                    return inPath;
                }
            }

            if (Platform.IsUnixy)
            {
                throw new NotSupportedException(
                    lookInPath
                        ? $"{nameof(TempRedis)} requires redis-server to be in your path for this platform. Is redis installed on this system? Searched folders:\n{string.Join("\n", Find.FoldersInPath)}"
                        : $"{nameof(TempRedis)} only supports finding redis-server in your path for this platform. Please enable the flag or pass in a path to redis-server."
                );
            }

            var lookForService = locatorStrategies.HasFlag(RedisLocatorStrategies.FindAsWindowsService);
            if (lookForService)
            {
                var serviceExePath = RedisWindowsServiceFinder.FindPathToRedis();
                if (serviceExePath is not null)
                {
                    return serviceExePath;
                }
            }

            var attemptDownload = locatorStrategies.HasFlag(RedisLocatorStrategies.DownloadForWindowsIfNecessary);
            if (!attemptDownload)
            {
                throw new NotSupportedException(
                    GenerateFailMessageFor(lookInPath, lookForService, false, null)
                );
            }

            var downloadError = "unknown";
            var result = Async.RunSync(
                () =>
                {
                    var downloader = new MicrosoftRedisDownloader();
                    try
                    {
                        return downloader.Fetch();
                    }
                    catch (Exception ex)
                    {
                        downloadError = ex.Message;
                        return null;
                    }
                }
            );

            if (result is not null)
            {
                return result;
            }

            throw new NotSupportedException(
                GenerateFailMessageFor(lookInPath, lookForService, true, downloadError)
            );
        }

        private static string GenerateFailMessageFor(
            bool lookInPath,
            bool lookForService,
            bool attemptDownload,
            string downloadError
        )
        {
            return new[]
            {
                "Unable to start up a temporary redis instance: no redis-server.exe could be found",
                lookInPath
                    ? "* Try adding the folder containing redis-server.exe to your path"
                    : "* Try enabling the FindInPath location strategy",
                lookForService
                    ? "* Try installing the Redis windows service (must be called 'redis' to be found)"
                    : "* Try enabling a search for a locally-installed Redis service",
                attemptDownload
                    ? $"* Unable to download Redis from github/Microsoft/archive: {downloadError}"
                    : "* Try enabling auto-download of Redis from github/Microsoft/archive"
            }.JoinWith("\n");
        }
    }
}