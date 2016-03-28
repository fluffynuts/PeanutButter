using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PeanutButter.FileSystem
{
    public class LocalFileSystem: IFileSystem
    {
        public IEnumerable<string> List(string path, string searchPattern = "*")
        {
            return ListFolders(path, searchPattern)
                    .Union(ListFiles(path, searchPattern));
        }

        public IEnumerable<string> ListFiles(string path, string searchPattern = "*")
        {
            return ListFilesInternal(path, searchPattern, SearchOption.TopDirectoryOnly);
        }

        public IEnumerable<string> ListFolders(string path, string searchPattern = "*")
        {
            return ListFoldersInternal(path, searchPattern, SearchOption.TopDirectoryOnly);
        }

        public IEnumerable<string> ListFilesRecursive(string path, string searchPattern = "*")
        {
            return ListFilesInternal(path, searchPattern, SearchOption.AllDirectories);
        }

        public IEnumerable<string> ListFoldersRecursive(string path, string searchPattern = "*")
        {
            return ListFoldersInternal(path, searchPattern, SearchOption.AllDirectories);
        }

        private static IEnumerable<string> ListFoldersInternal(string path, 
                                                                string searchPattern,
                                                                SearchOption searchOption)
        {
            var startIndex = path.Length + 1;
            return Directory.EnumerateDirectories(path, searchPattern, searchOption)
                .Select(p => p.Substring(startIndex));
        }

        private static IEnumerable<string> ListFilesInternal(string path, 
                                                                string searchPattern,
                                                                SearchOption searchOption)
        {
            var startIndex = path.Length + 1;
            return Directory.EnumerateFiles(path, searchPattern, searchOption)
                .Select(p => p.Substring(startIndex));
        }
    }
}

