using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PeanutButter.FileSystem
{
    public class LocalFileSystem: IFileSystem
    {
        public IEnumerable<string> List(string path, string searchPattern = "*")
        {
            var startIndex = path.Length + 1;
            return Directory.EnumerateDirectories(path, searchPattern)
                            .Select(p => p.Substring(startIndex))
                            .Union(ListFiles(path, searchPattern));
        }

        public IEnumerable<string> ListFiles(string path, string searchPattern = "*")
        {
            var startIndex = path.Length + 1;
            return Directory.EnumerateFiles(path, searchPattern)
                            .Select(p => p.Substring(startIndex));
        }
    }
}

