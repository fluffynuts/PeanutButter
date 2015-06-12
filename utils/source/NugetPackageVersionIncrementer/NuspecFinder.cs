using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NugetPackageVersionIncrementer
{
    public class NuspecFinder
    {
        private readonly List<string> _foundNuspecPaths;
        public IEnumerable<string> NuspecPaths { get { return _foundNuspecPaths; } }
        public NuspecFinder(string basePath)
        {
            if (!Directory.Exists(basePath))
                throw new ArgumentException(basePath + " not found", "basePath");
            _foundNuspecPaths = new List<string>();
            FindNuspecsUnder(basePath);
        }

        private void FindNuspecsUnder(string basePath)
        {
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
