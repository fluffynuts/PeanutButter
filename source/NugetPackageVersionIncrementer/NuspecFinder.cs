using System;
using System.Collections.Generic;
using System.IO;

namespace NugetPackageVersionIncrementer
{
    public interface INuspecFinder
    {
        string[] NuspecPaths { get; }
        void FindNuspecsUnder(string basePath);
    }

    public class NuspecFinder : INuspecFinder
    {
        public string[] NuspecPaths => _foundNuspecPaths?.ToArray() ?? Array.Empty<string>();
        private readonly List<string> _foundNuspecPaths = new();

        public void FindNuspecsUnder(string basePath)
        {
            if (basePath is null)
            {
                throw new ArgumentNullException(nameof(basePath));
            }

            if (!Directory.Exists(basePath))
            {
                throw new ArgumentException(
                    $"{basePath} not found",
                    nameof(basePath)
                );
            }

            _foundNuspecPaths.AddRange(
                Directory.EnumerateFileSystemEntries(
                    basePath,
                    "*.nuspec",
                    SearchOption.AllDirectories
                )
            );
        }
    }
}