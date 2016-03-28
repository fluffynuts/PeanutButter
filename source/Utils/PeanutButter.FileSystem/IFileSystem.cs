using System.Collections.Generic;

namespace PeanutButter.FileSystem
{
    public interface IFileSystem
    {
        IEnumerable<string> List(string path, string searchPattern = "*");
        IEnumerable<string> ListFiles(string path, string searchPattern = "*");
        IEnumerable<string> ListFolders(string path, string searchPattern = "*");
        IEnumerable<string> ListRecursive(string path, string searchPattern = "*");
        IEnumerable<string> ListFilesRecursive(string path, string searchPattern = "*");
        IEnumerable<string> ListFoldersRecursive(string path, string searchPattern = "*");
    }
}