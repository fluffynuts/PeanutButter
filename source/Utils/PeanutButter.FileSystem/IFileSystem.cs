using System.Collections.Generic;

namespace PeanutButter.FileSystem
{
    public interface IFileSystem
    {
        IEnumerable<string> List(string path, string searchPattern = "*");
        IEnumerable<string> ListFiles(string path, string searchPattern = "*");
    }
}