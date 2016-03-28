using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PeanutButter.FileSystem
{
    public class LocalFileSystem: IFileSystem
    {
        private string _currentDirectory;

        public LocalFileSystem()
        {
            SetCurrentDirectory(Directory.GetCurrentDirectory());
        }
        public IEnumerable<string> List(string searchPattern = "*")
        {
            var path = _currentDirectory;
            var startIndex = path.Length + 1;
            return Directory.EnumerateFileSystemEntries(path, searchPattern, SearchOption.TopDirectoryOnly)
                            .Select(p => p.Substring(startIndex));
        }

        public IEnumerable<string> ListFiles(string searchPattern = "*")
        {
            return ListFilesInternal(_currentDirectory, searchPattern, SearchOption.TopDirectoryOnly);
        }

        public IEnumerable<string> ListDirectories(string searchPattern = "*")
        {
            return ListDirectoriesInternal(_currentDirectory, searchPattern, SearchOption.TopDirectoryOnly);
        }

        public IEnumerable<string> ListRecursive(string searchPattern = "*")
        {
            var path = _currentDirectory;
            var startIndex = path.Length + 1;
            return Directory.EnumerateFileSystemEntries(path, searchPattern, SearchOption.AllDirectories)
                            .Select(p => p.Substring(startIndex));
        }

        public IEnumerable<string> ListFilesRecursive(string searchPattern = "*")
        {
            return ListFilesInternal(_currentDirectory, searchPattern, SearchOption.AllDirectories);
        }

        public IEnumerable<string> ListDirectoriesRecursive(string searchPattern = "*")
        {
            return ListDirectoriesInternal(_currentDirectory, searchPattern, SearchOption.AllDirectories);
        }

        public string GetCurrentDirectory()
        {
            return _currentDirectory;
        }

        public void SetCurrentDirectory(string path)
        {
            _currentDirectory = path;
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

