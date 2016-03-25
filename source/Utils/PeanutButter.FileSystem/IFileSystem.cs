using System.Collections.Generic;

namespace PeanutButter.FileSystem
{
    public interface IFileSystem
    {
        IEnumerable<string> List(string path);
        IEnumerable<string> List(string path, string searchPattern); 
    }
}