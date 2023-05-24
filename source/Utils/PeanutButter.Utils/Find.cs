using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// Finds files
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        static class Find
    {
        private static readonly string PathItemSeparator = Platform.IsUnixy
            ? ":"
            : ";";

        /// <summary>
        /// Produces a fresh list of the folders in your path according
        /// to the current value of the PATH environment variable on
        /// every invocation.
        /// </summary>
        public static string[] FoldersInPath =>
            (Environment.GetEnvironmentVariable("PATH") ?? "").Split(
                new[] { PathItemSeparator },
                StringSplitOptions.RemoveEmptyEntries
            );

        /// <summary>
        /// Finds the first match for a given filename in the PATH
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public static string InPath(string search)
        {
            var extensions = Platform.IsUnixy
                ? new string[0]
                : GenerateWindowsExecutableExtensionsList();
            return FoldersInPath.Aggregate(
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

            var script = @$"if test -x ""{filePath}""; then
    exit 0
else
    exit 1
fi";
            using var tmp = new AutoTempFile();
            File.WriteAllText(tmp.Path, script);
            using var io = ProcessIO.Start("sh", tmp.Path);
            if (!io.Started)
            {
                return null;
            }

            io.WaitForExit();
            return io.ExitCode == 0
                ? filePath
                : null;
        }

        private static string[] GenerateWindowsExecutableExtensionsList()
        {
            return (Environment.GetEnvironmentVariable("PATHEXT") ?? "")
                .Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static string SearchFor(
            string search,
            string inFolder,
            IEnumerable<string> withExecutableExtensions
        )
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
                }
            );
        }
    }
}