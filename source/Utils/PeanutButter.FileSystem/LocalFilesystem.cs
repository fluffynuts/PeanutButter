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

        public void Delete(string path)
        {
            DeleteInternal(path, false);
        }

        public void DeleteRecursive(string path)
        {
            DeleteInternal(path, true);
        }

        public void Copy(string sourcePath)
        {
            Copy(GetFullPathFor(sourcePath), Path.GetFileName(sourcePath));
        }

        public void Copy(string sourcePath, string targetRelativePath)
        {
            var currentPath = _currentDirectory;
            var absoluteTarget = Path.Combine(currentPath, targetRelativePath);
            EnsureFolderExists(Path.GetDirectoryName(absoluteTarget));
            File.Copy(sourcePath, absoluteTarget);
        }

        public Stream OpenReader(string path)
        {
            var fullPath = GetFullPathFor(path);
            return File.Exists(fullPath)
                        ? File.OpenRead(fullPath)
                        : null;
        }

        public Stream OpenWriter(string path)
        {
            var fullPath = GetFullPathFor(path);
            return File.OpenWrite(fullPath);
        }

        public string GetFullPathFor(string relativePath)
        {
            return Path.IsPathRooted(relativePath) 
                        ? relativePath 
                        : Path.Combine(_currentDirectory, relativePath);
        }

        private void EnsureFolderExists(string folderPath)
        {
            if (Directory.Exists(folderPath))
                return;
            Directory.CreateDirectory(folderPath);
        }


        private void DeleteInternal(string path, bool recursive)
        {
            path = Path.Combine(_currentDirectory, path);
            if (File.Exists(path))
                File.Delete(path);
            else if (Directory.Exists(path))
                Directory.Delete(path, recursive);
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

