using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PeanutButter.FileSystem
{
    public class LocalFileSystem: IFileSystem
    {
        public IEnumerable<string> List(string path)
        {
            return List(path, "*");
        }

        public IEnumerable<string> List(string path, string searchPattern)
        {
            var startIndex = path.Length + 1;
            return Directory.EnumerateDirectories(path, searchPattern)
                            .Union(Directory.EnumerateFiles(path, searchPattern))
                            .Select(p => p.Substring(startIndex));
        }
    }
}
