using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Finds files
    /// </summary>
    public static class Find
    {
        private static readonly string PathItemSeparator = Platform.IsUnixy ? ":" : ";";

        /// <summary>
        /// Finds the first match for a given filename in the PATH
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public static string InPath(string search)
        {
            var paths = (Environment.GetEnvironmentVariable("PATH") ?? "").Split(
                new[] { PathItemSeparator },
                StringSplitOptions.RemoveEmptyEntries);
            var extensions = Platform.IsUnixy ? new string[0] : GenerateWindowsExecutableExtensionsList();
            return paths.Aggregate(
                null as string,
                (acc, cur) => acc ?? ValidateExecutable(SearchFor(search, cur, extensions))
            );
        }

        private static string ValidateExecutable(string filePath)
        {
            if (filePath == null || !Platform.IsUnixy)
            {
                return filePath;
            }

            return null;
        }

        private static string[] GenerateWindowsExecutableExtensionsList()
        {
            return (Environment.GetEnvironmentVariable("PATHEXT") ?? "")
                .Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static string SearchFor(
            string search,
            string inFolder,
            IEnumerable<string> withExecutableExtensions)
        {
            var fullPath = Path.Combine(inFolder, search);
            if (File.Exists(fullPath))
            {
                // this is well imperfect: I don't know how to test for executability :/
                return fullPath;
            }

            return withExecutableExtensions.Aggregate(
                null as string,
                (acc, cur) =>
                {
                    if (acc != null)
                    {
                        return acc;
                    }

                    var thisTest = $"{fullPath}{cur}";
                    return File.Exists(thisTest)
                        ? thisTest
                        : null;
                });
        }
    }
}