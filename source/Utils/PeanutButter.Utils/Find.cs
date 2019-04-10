using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PeanutButter.Utils
{
    public class Find
    {
        private static PlatformID[] UnixOperatingSystems =
        {
            PlatformID.Unix,
            PlatformID.MacOSX
        };

        private static bool IsUnixy => UnixOperatingSystems.Contains(Environment.OSVersion.Platform);

        private static readonly string PathItemSeparator = IsUnixy ? ":" : ";";

        public static string InPath(string search)
        {
            var paths = (Environment.GetEnvironmentVariable("PATH") ?? "").Split(
                new[] {PathItemSeparator},
                StringSplitOptions.RemoveEmptyEntries);
            var extensions = IsUnixy ? new string[0] : GenerateWindowsExecutableExtensionsList();
            return paths.Aggregate(
                null as string,
                (acc, cur) => acc ?? SearchFor(search, cur, extensions)
            );
        }

        private static string[] GenerateWindowsExecutableExtensionsList()
        {
            return (Environment.GetEnvironmentVariable("PATHEXT") ?? "")
                .Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries);
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