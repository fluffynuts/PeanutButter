using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Utility class to provide the ability to enumerate files without throwing exceptions on access errors
    /// </summary>
    public static class SafeWalk
    {
        /// <summary>
        /// mostly lifted from http://stackoverflow.com/questions/5098011/directory-enumeratefiles-unauthorizedaccessexception 
        /// </summary>
        /// <param name="path">Base path to start the search from</param>
        /// <param name="searchPattern">Filename pattern to match for result files</param>
        /// <param name="searchOpt">Whether to search just the given base folder or recurse down the path tree</param>
        /// <returns>A collection of strings which are the found file paths. May be empty, will not throw, even on access errors.</returns>
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