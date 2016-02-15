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
            AddNuspecsIn(basePath);
            foreach (var dir in Directory.GetDirectories(basePath))
            {
                FindNuspecsUnder(dir);
            }
        }

        private void AddNuspecsIn(string FolderPath)
        {
            foreach (var file in Directory.GetFiles(FolderPath, "*.nuspec"))
            {
                _foundNuspecPaths.Add(file);
            }
        }
    }
}
