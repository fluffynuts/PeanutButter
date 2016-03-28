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
            return Directory.EnumerateFileSystemEntries(path, searchPattern, SearchOption.TopDirectoryOnly)
                            .Select(p => p.Substring(startIndex));
        }

        public IEnumerable<string> ListFiles(string path, string searchPattern = "*")
        {
            return ListFilesInternal(path, searchPattern, SearchOption.TopDirectoryOnly);
        }

        public IEnumerable<string> ListDirectories(string path, string searchPattern = "*")
        {
            return ListDirectoriesInternal(path, searchPattern, SearchOption.TopDirectoryOnly);
        }

        public IEnumerable<string> ListRecursive(string path, string searchPattern = "*")
        {
            var startIndex = path.Length + 1;
            return Directory.EnumerateFileSystemEntries(path, searchPattern, SearchOption.AllDirectories)
                            .Select(p => p.Substring(startIndex));
        }

        public IEnumerable<string> ListFilesRecursive(string path, string searchPattern = "*")
        {
            return ListFilesInternal(path, searchPattern, SearchOption.AllDirectories);
        }

        public IEnumerable<string> ListDirectoriesRecursive(string path, string searchPattern = "*")
        {
            return ListDirectoriesInternal(path, searchPattern, SearchOption.AllDirectories);
        }

        private static IEnumerable<string> ListDirectoriesInternal(string path, 
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

