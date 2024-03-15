using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PeanutButter.Utils;

/// <summary>
/// Provides a service to resolve path files, including
/// with globbing
/// </summary>
public interface IFilePathResolver
{
    /// <summary>
    /// Given a bunch of patterns or absolute paths,
    /// will return back file entries which match the
    /// given paths/patterns
    /// </summary>
    /// <param name="patterns"></param>
    /// <returns></returns>
    IEnumerable<string> Resolve(params string[] patterns);
}

/// <inheritdoc />
public class FilePathResolver : IFilePathResolver
{
    /// <inheritdoc />
    public IEnumerable<string> Resolve(params string[] patterns)
    {
        foreach (var item in patterns)
        {
            if (File.Exists(item))
            {
                yield return item;
                continue;
            }

            if (Directory.Exists(item))
            {
                foreach (var sub in ListDir(item, "*", recurse: true))
                {
                    yield return sub;
                }

                continue;
            }

            var parts = item.SplitPath();
            if (!parts.Any(p => p.Contains("*")))
            {
                continue;
            }

            foreach (var p1 in SearchWithFilter(parts))
            {
                yield return p1;
            }
        }
    }

    private IEnumerable<string> SearchWithFilter(
        string[] pathParts
    )
    {
        var startParts = new List<string>();
        var queue = new Queue<string>(pathParts);
        while (queue.Any())
        {
            var current = queue.Peek();
            if (current.Contains("*"))
            {
                break;
            }

            startParts.Add(queue.Dequeue());
        }

        var start = startParts.Any()
            ? startParts.JoinPath()
            : Environment.CurrentDirectory;
        var unfiltered = ListDir(start, "*", recurse: true);
        var rawFilter = queue.JoinPath();
        var filter = new Regex(
            @$"^{
                rawFilter.Replace("*", ".*")
                    .Replace("\\", "\\\\")
                    .Replace("/", "\\/")
            }$"
        );
        foreach (var test in unfiltered)
        {
            var relative = test.Replace(start, "");
            if (
                relative.StartsWith($"{Path.DirectorySeparatorChar}") ||
                relative.StartsWith($"{Path.AltDirectorySeparatorChar}")
            )
            {
                relative = relative.Substring(1);
            }

            if (filter.IsMatch(relative))
            {
                yield return test;
            }
        }
    }

    private IEnumerable<string> ListDir(
        string item,
        string searchPattern,
        bool recurse
    )
    {
        foreach (
            var sub in Directory.EnumerateFileSystemEntries(
                item,
                searchPattern
            )
        )
        {
            if (File.Exists(sub))
            {
                yield return sub;
            }

            if (recurse && Directory.Exists(sub))
            {
                foreach (var subItem in ListDir(sub, searchPattern, recurse))
                {
                    yield return subItem;
                }
            }
        }
    }
}