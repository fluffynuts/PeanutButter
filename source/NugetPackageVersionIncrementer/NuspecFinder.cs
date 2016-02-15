using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NugetPackageVersionIncrementer
{
    public interface INuspecFinder
    {
        IEnumerable<string> NuspecPaths { get; }
        void FindNuspecsUnder(string basePath);
    }

    public class NuspecFinder : INuspecFinder
    {
        public IEnumerable<string> NuspecPaths => _foundNuspecPaths ?? new string[] { }.AsEnumerable();
        private List<string> _foundNuspecPaths = new List<string>();

        public void FindNuspecsUnder(string basePath)
        {
            if (!Directory.Exists(basePath))
                throw new ArgumentException(basePath + " not found", nameof(basePath));
            _foundNuspecPaths.AddRange(Directory.EnumerateFileSystemEntries(basePath, "*.nuspec", SearchOption.AllDirectories));
        }

    }
}
