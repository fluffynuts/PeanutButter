using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PeanutButter.Utils
{
    public static class SafeWalk
    {
        // mostly lifted from http://stackoverflow.com/questions/5098011/directory-enumeratefiles-unauthorizedaccessexception 
        public static IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOpt)
        {   
            try
            {
                var dirFiles = Enumerable.Empty<string>();
                if(searchOpt == SearchOption.AllDirectories)
                {
                    dirFiles = Directory.EnumerateDirectories(path)
                        .SelectMany(x => EnumerateFiles(x, searchPattern, searchOpt));
                }
                return dirFiles.Concat(Directory.EnumerateFiles(path, searchPattern));
            }
            catch
            {
                return Enumerable.Empty<string>();
            }
        }
    }
}