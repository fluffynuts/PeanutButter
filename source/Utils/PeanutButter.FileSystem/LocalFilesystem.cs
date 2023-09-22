using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PeanutButter.FileSystem;

public class LocalFileSystem : IFileSystem
{
    private string _currentDirectory;

    public LocalFileSystem()
    {
        SetCurrentDirectory(Directory.GetCurrentDirectory());
    }

    public LocalFileSystem(string start)
    {
        SetCurrentDirectory(start);
    }

    public IEnumerable<string> List(
        string searchPattern = "*"
    )
    {
        var path = _currentDirectory;
        var startIndex = path.Length + 1;
        return Directory.EnumerateFileSystemEntries(path, searchPattern, SearchOption.TopDirectoryOnly)
            .Select(p => p.Substring(startIndex));
    }

    public IEnumerable<string> ListFiles(
        string searchPattern = "*"
    )
    {
        return ListFilesInternal(
            _currentDirectory,
            searchPattern,
            SearchOption.TopDirectoryOnly
        );
    }

    public IEnumerable<string> ListDirectories(
        string searchPattern = "*"
    )
    {
        return ListDirectoriesInternal(
            _currentDirectory,
            searchPattern,
            SearchOption.TopDirectoryOnly
        );
    }

    public IEnumerable<string> ListRecursive(
        string searchPattern = "*"
    )
    {
        var path = _currentDirectory;
        var startIndex = path.Length + 1;
        return Directory.EnumerateFileSystemEntries(
                path,
                searchPattern,
                SearchOption.AllDirectories
            )
            .Select(p => p.Substring(startIndex));
    }

    public IEnumerable<string> ListFilesRecursive(
        string searchPattern = "*"
    )
    {
        return ListFilesInternal(
            _currentDirectory,
            searchPattern,
            SearchOption.AllDirectories
        );
    }

    public IEnumerable<string> ListDirectoriesRecursive(
        string searchPattern = "*"
    )
    {
        return ListDirectoriesInternal(
            _currentDirectory,
            searchPattern,
            SearchOption.AllDirectories
        );
    }

    public string GetCurrentDirectory()
    {
        return _currentDirectory;
    }

    public void SetCurrentDirectory(
        string path
    )
    {
        _currentDirectory = path;
    }

    public void Delete(
        string path
    )
    {
        DeleteInternal(path, false);
    }

    public void DeleteRecursive(
        string path
    )
    {
        DeleteInternal(
            path,
            true
        );
    }

    public void Copy(
        string sourcePath
    )
    {
        var fullPath = GetFullPathFor(sourcePath);
        Copy(
            GetFullPathFor(
                sourcePath
            ),
            Path.GetFileName(sourcePath)
        );
    }

    public void Copy(
        string sourcePath,
        string targetPath
    )
    {
        var absoluteSource = ResolveAbsolutePath(sourcePath);
        var absoluteTarget = ResolveAbsolutePath(targetPath);
        if (DirectoryExists(absoluteSource))
        {
            CopyTree(absoluteSource, absoluteTarget);
        }
        else if (FileExists(absoluteSource))
        {
            CopyFile(absoluteSource, absoluteTarget);
        }
        else
        {
            throw new PathNotFoundException(sourcePath);
        }
    }

    private void CopyTree(
        string src,
        string target
    )
    {
        if (Directory.Exists(target))
        {
            target = Path.Combine(
                target,
                Path.GetFileName(src)
            );
        }

        EnsureFolderExists(target);
        CopyContents(src, target);
    }

    private string ResolveAbsolutePath(
        string path
    )
    {
        return Path.IsPathRooted(path)
            ? path
            : Path.Combine(_currentDirectory, path);
    }

    private void CopyContents(
        string fromFolder,
        string toFolder
    )
    {
        CopyFiles(fromFolder, toFolder);
        CopyFolders(fromFolder, toFolder);
    }

    private void CopyFiles(
        string fromFolder,
        string toFolder
    )
    {
        foreach (var f in Directory.GetFiles(fromFolder))
        {
            var baseName = Path.GetFileName(f);
            CopyFile(
                f,
                Path.Combine(toFolder, baseName)
            );
        }
    }

    private void CopyFolders(
        string fromFolder,
        string toFolder
    )
    {
        foreach (var d in Directory.GetDirectories(fromFolder))
        {
            var basename = Path.GetFileName(d);
            var target = Path.Combine(toFolder, basename);
            EnsureFolderExists(target);
            CopyContents(d, target);
        }
    }

    private static void CopyFile(
        string src,
        string target
    )
    {
        EnsureFolderExists(Path.GetDirectoryName(target));
        File.Copy(
            src,
            target
        );
    }

    public Stream OpenReader(
        string path
    )
    {
        var fullPath = GetFullPathFor(path);
        return File.Exists(fullPath)
            ? File.OpenRead(fullPath)
            : null;
    }

    public Stream OpenWriter(
        string path
    )
    {
        var fullPath = GetFullPathFor(path);
        return File.OpenWrite(fullPath);
    }

    public Stream Open(
        string targetPath
    )
    {
        return File.Open(
            GetFullPathFor(
                targetPath
            ),
            FileMode.Open,
            FileAccess.ReadWrite
        );
    }

    public void Move(
        string source,
        string target,
        bool overwrite = false
    )
    {
        var absoluteSource = GetFullPathFor(source);
        var absoluteTarget = GetFullPathFor(target);
        var targetFolder = Path.GetDirectoryName(absoluteTarget);
        EnsureFolderExists(targetFolder);
        if (overwrite && File.Exists(absoluteTarget))
        {
            File.Delete(absoluteTarget);
        }

        File.Move(absoluteSource, absoluteTarget);
    }

    public string GetFullPathFor(
        string relativePath
    )
    {
        return Path.IsPathRooted(relativePath)
            ? relativePath
            : Path.Combine(_currentDirectory, relativePath);
    }

    private static void EnsureFolderExists(
        string folderPath
    )
    {
        if (Directory.Exists(folderPath))
        {
            return;
        }

        Directory.CreateDirectory(folderPath);
    }

    private void DeleteInternal(
        string path,
        bool recursive
    )
    {
        path = Path.Combine(_currentDirectory, path);
        if (File.Exists(path))
        {
            File.Delete(path);
            return;
        }

        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive);
        }
    }

    private static IEnumerable<string> ListDirectoriesInternal(
        string path,
        string searchPattern,
        SearchOption searchOption
    )
    {
        var startIndex = path.Length + 1;
        return Directory.EnumerateDirectories(path, searchPattern, searchOption)
            .Select(p => p.Substring(startIndex))
            .ToArray();;
    }

    private static IEnumerable<string> ListFilesInternal(
        string path,
        string searchPattern,
        SearchOption searchOption
    )
    {
        var startIndex = path.Length + 1;
        return Directory.EnumerateFiles(path, searchPattern, searchOption)
            .Select(p => p.Substring(startIndex))
            .ToArray();
    }

    /// <inheritdoc />
    public bool FileExists(
        string path
    )
    {
        return File.Exists(
            ResolveAbsolutePath(
                path
            )
        );
    }

    public bool DirectoryExists(
        string path
    )
    {
        return Directory.Exists(
            ResolveAbsolutePath(
                path
            )
        );
    }
}