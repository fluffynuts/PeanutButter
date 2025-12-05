using System.Collections.Generic;
using System.IO;

namespace PeanutButter.FileSystem;

/// <summary>
/// Provides a wrapper around a tree within a filesystem
/// </summary>
public interface IFileSystem
{
    /// <summary>
    /// List any items within the root of the current filesystem,
    /// with an optional search pattern
    /// </summary>
    /// <param name="searchPattern"></param>
    /// <returns></returns>
    IEnumerable<string> List(string searchPattern = "*");

    /// <summary>
    /// List files within the root of the current filesystem,
    /// with an optional search pattern
    /// </summary>
    /// <param name="searchPattern"></param>
    /// <returns></returns>
    IEnumerable<string> ListFiles(string searchPattern = "*");

    /// <summary>
    /// List directories within the root of the current filesystem,
    /// with an optional search pattern
    /// </summary>
    /// <param name="searchPattern"></param>
    /// <returns></returns>
    IEnumerable<string> ListFolders(string searchPattern = "*");

    /// <summary>
    /// List any any items recursively from the root of the current
    /// filesystem downward, with an optional search pattern
    /// </summary>
    /// <param name="searchPattern"></param>
    /// <returns></returns>
    IEnumerable<string> ListRecursive(string searchPattern = "*");

    /// <summary>
    /// List files items recursively from the root of the current
    /// filesystem downward, with an optional search pattern
    /// </summary>
    /// <param name="searchPattern"></param>
    /// <returns></returns>
    IEnumerable<string> ListFilesRecursive(string searchPattern = "*");

    /// <summary>
    /// List folders items recursively from the root of the current
    /// filesystem downward, with an optional search pattern
    /// </summary>
    /// <param name="searchPattern"></param>
    /// <returns></returns>
    IEnumerable<string> ListFoldersRecursive(string searchPattern = "*");

    /// <summary>
    /// Retrieves the current working directory within the filesystem
    /// </summary>
    /// <returns></returns>
    string GetCurrentDirectory();

    /// <summary>
    /// Set the current working directory within the filesystem
    /// </summary>
    /// <param name="path"></param>
    void SetCurrentDirectory(string path);

    /// <summary>
    /// Delete a folder or file within the filesystem
    /// </summary>
    /// <param name="path"></param>
    void Delete(string path);

    /// <summary>
    /// Recursively delete a folder (or nuke a file)
    /// </summary>
    /// <param name="path"></param>
    void DeleteRecursive(string path);

    /// <summary>
    /// Copy the file at the provided sourcePath into the current folder
    /// </summary>
    /// <param name="sourcePath"></param>
    void Copy(string sourcePath);

    /// <summary>
    /// Copy the file at the source path to the target path
    /// </summary>
    /// <param name="sourcePath"></param>
    /// <param name="targetPath"></param>
    void Copy(string sourcePath, string targetPath);

    /// <summary>
    /// Open a stream-reader to the provided file path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    Stream OpenReader(string path);

    /// <summary>
    /// Open a stream-writer to the provided file path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    Stream OpenWriter(string path);

    /// <summary>
    /// Open a target path with all options (read-write) without
    /// destroying it.
    /// </summary>
    /// <param name="targetPath"></param>
    /// <returns></returns>
    Stream Open(string targetPath);

    /// <summary>
    /// Move a file or
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <param name="overwrite"></param>
    void Move(string source, string target, bool overwrite = false);

    /// <summary>
    /// Test if a file exists
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    bool FileExists(string path);

    /// <summary>
    /// Test if a folder exists
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    bool FolderExists(string path);

    /// <summary>
    /// Provides the full local path for the provided relative path
    /// </summary>
    /// <param name="relativePath"></param>
    /// <returns></returns>
    string GetFullPathFor(
        string relativePath
    );
}