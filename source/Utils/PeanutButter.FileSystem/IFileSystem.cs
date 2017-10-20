using System.Collections.Generic;
using System.IO;

namespace PeanutButter.FileSystem
{
    public interface IFileSystem
    {
        IEnumerable<string> List(string searchPattern = "*");
        IEnumerable<string> ListFiles(string searchPattern = "*");
        IEnumerable<string> ListDirectories(string searchPattern = "*");
        IEnumerable<string> ListRecursive(string searchPattern = "*");
        IEnumerable<string> ListFilesRecursive(string searchPattern = "*");
        IEnumerable<string> ListDirectoriesRecursive(string searchPattern = "*");
        string GetCurrentDirectory();
        void SetCurrentDirectory(string path);
        void Delete(string path);
        void DeleteRecursive(string path);
        void Copy(string sourcePath);
        void Copy(string sourcePath, string targetRelativePath);
        Stream OpenReader(string path);
        Stream OpenWriter(string path);
        Stream Open(string targetPath);
        void Move(string source, string target, bool overwrite = false);
    }
}