using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.Utils
{
    /// <summary>
    /// Describes the contract for an auto-deleting temporary folder
    /// </summary>
    public interface IAutoTempFolder : IDisposable
    {
        /// <summary>
        /// The local path at which this temp folder exists
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Resolves a relative path (or path parts) within the auto temp folder
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="more"></param>
        /// <returns></returns>
        string ResolvePath(
            string p1,
            params string[] more
        );

        /// <summary>
        /// Creates a directory within the auto temp folder
        /// - any supporting directories will also be created
        /// </summary>
        /// <param name="dirname"></param>
        /// <returns></returns>
        string CreateFolder(string dirname);

        /// <summary>
        /// Writes a file within the auto temp folder
        /// </summary>
        /// <param name="filename">relative path to the file</param>
        /// <param name="data">string data to write</param>
        /// <returns>full path to the written file</returns>
        string WriteFile(
            string filename,
            string data
        );

        /// <summary>
        /// Writes a file within the auto temp folder
        /// </summary>
        /// <param name="filename">relative path to the file</param>
        /// <param name="data">binary data to write</param>
        /// <returns>full path to the written file</returns>
        string WriteFile(
            string filename,
            byte[] data
        );

        /// <summary>
        /// Writes a file within the auto temp folder
        /// </summary>
        /// <param name="filename">relative path to the file</param>
        /// <param name="data">stream data to write</param>
        /// <returns>full path to the written file</returns>
        string WriteFile(
            string filename,
            Stream data
        );

        /// <summary>
        /// Reads a text file by relative path within the auto temp folder
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        string ReadTextFile(string filename);

        /// <summary>
        /// Reads a binary file by relative path within the auto temp folder
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        byte[] ReadFile(string filename);

        /// <summary>
        /// Opens a file (read-only) by relative path within the auto temp folder
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        FileStream OpenFile(string filename);

        /// <summary>
        /// Opens a file by relative path within the auto temp folder
        /// with the desired access
        /// - defaulted share is None
        /// - defaulted buffer size is 4096
        /// - defaulted options is None
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="access"></param>
        /// <returns></returns>
        FileStream OpenFile(
            string filename,
            FileAccess access
        );

        /// <summary>
        /// Opens a file by relative path within the auto temp folder
        /// with the desired access and share
        /// - defaulted buffer size is 4096
        /// - defaulted options is None
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="access"></param>
        /// <param name="share"></param>
        FileStream OpenFile(
            string filename,
            FileAccess access,
            FileShare share
        );

        /// <summary>
        /// Opens a file by relative path within the auto temp folder
        /// with the desired access, share and buffer size
        /// - defaulted options is None
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="access"></param>
        /// <param name="share"></param>
        /// <param name="bufferSize"></param>
        FileStream OpenFile(
            string filename,
            FileAccess access,
            FileShare share,
            int bufferSize
        );

        /// <summary>
        /// Opens a file by relative path within the auto temp folder
        /// with the desired access, share, buffer size and options
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="access"></param>
        /// <param name="share"></param>
        /// <param name="bufferSize"></param>
        /// <param name="options"></param>
        FileStream OpenFile(
            string filename,
            FileAccess access,
            FileShare share,
            int bufferSize,
            FileOptions options
        );

        /// <summary>
        /// Returns true if the relative path is found to be a file
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        bool FileExists(string relativePath);

        /// <summary>
        /// Returns true if the relative path is found to be a folder
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        bool FolderExists(string relativePath);

        /// <summary>
        /// Returns true if the relative path is a file or folder
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        bool Exists(string relativePath);

        /// <summary>
        /// Returns true if the provided path is contained within the temp folder
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        bool Contains(string fullPath);
    }

    /// <summary>
    /// Provides a mechanism for creating a temporary folder which is automatically
    /// deleted upon disposal.
    /// </summary>
    public class AutoTempFolder : IAutoTempFolder
    {
        /// <summary>
        /// Default file access, as per System.IO.FileStream
        /// </summary>
        public const FileAccess DEFAULT_FILE_ACCESS = FileAccess.Read;

        /// <summary>
        /// Default file share mode as per System.IO.FileStream
        /// </summary>
        public const FileShare DEFAULT_FILE_SHARE = FileShare.None;

        /// <summary>
        /// Default buffer size as per System.IO.FileStream
        /// </summary>
        public const int DEFAULT_BUFFER_SIZE = 4096; // this is the default in FileStream.cs

        /// <summary>
        /// Default file options, as per System.IO.FileStream
        /// </summary>
        public const FileOptions DEFAULT_FILE_OPTIONS = FileOptions.None;

        /// <inheritdoc />
        public string Path { get; private set; }

        private readonly object _lock = new object();
        private AutoDeleter _autoDeleter;

        /// <summary>
        /// Default constructor: uses the operating system method to
        /// get a temporary path to use for the folder
        /// </summary>
        public AutoTempFolder() : this(System.IO.Path.GetTempPath())
        {
        }

        /// <summary>
        /// Constructs a new AutoTempFolder with the temporary folder
        /// housed under the provided baseFolder
        /// </summary>
        /// <param name="baseFolder">Folder within which to create the temporary folder</param>
        public AutoTempFolder(string baseFolder)
        {
            do
            {
                Path = System.IO.Path.Combine(baseFolder, Guid.NewGuid().ToString());
            } while (Directory.Exists(Path));

            Directory.CreateDirectory(Path);
            _autoDeleter = new AutoDeleter(Path);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            lock (_lock)
            {
                lock (_fileStreams)
                {
                    foreach (var stream in _fileStreams)
                    {
                        stream.Dispose();
                    }

                    _fileStreams.Clear();
                }

                _autoDeleter?.Dispose();
                _autoDeleter = null;
            }
        }

        /// <inheritdoc />
        public string ResolvePath(
            string p1,
            params string[] more
        )
        {
            var relative = string.Join(
                PathSeparator,
                new[] { p1 }.Concat(more)
            );
            if (Contains(relative))
            {
                return relative;
            }

            return string.Join(
                PathSeparator,
                new[] { Path, p1 }.Concat(more)
            );
        }

        private static readonly string PathSeparator
            = System.IO.Path.DirectorySeparatorChar.ToString();

        /// <inheritdoc />
        public string CreateFolder(string dirname)
        {
            var target = ResolvePath(dirname);
            if (!Directory.Exists(target))
            {
                Directory.CreateDirectory(target);
            }

            return target;
        }

        /// <inheritdoc />
        public string WriteFile(
            string filename,
            string data
        )
        {
            return WriteFile(
                filename,
                Encoding.UTF8.GetBytes(data)
            );
        }

        /// <inheritdoc />
        public string WriteFile(
            string filename,
            byte[] data
        )
        {
            return WriteFile(
                filename,
                new MemoryStream(data)
            );
        }

        /// <inheritdoc />
        public string WriteFile(
            string filename,
            Stream data
        )
        {
            var result = ResolvePath(filename);
            var container = System.IO.Path.GetDirectoryName(result);
            if (container is null)
            {
                throw new InvalidOperationException(
                    $"Unable to determine the containing folder for {result}"
                );
            }

            if (!Directory.Exists(container))
            {
                Directory.CreateDirectory(container);
            }

            using var filestream = new FileStream(result, FileMode.Create);
            data.CopyTo(filestream);
            filestream.Flush();
            return result;
        }

        /// <inheritdoc />
        public string ReadTextFile(string filename)
        {
            var data = ReadFile(filename);
            return Encoding.UTF8.GetString(data);
        }

        /// <inheritdoc />
        public byte[] ReadFile(string filename)
        {
            using var fp = OpenFile(filename, FileAccess.Read);
            var target = new MemoryStream();
            fp.CopyTo(target);
            return target.ToArray();
        }

        /// <inheritdoc />
        public FileStream OpenFile(string filename)
        {
            return OpenFile(filename, DEFAULT_FILE_ACCESS);
        }

        /// <inheritdoc />
        public FileStream OpenFile(string filename, FileAccess access)
        {
            return OpenFile(
                filename,
                access,
                DEFAULT_FILE_SHARE
            );
        }


        /// <inheritdoc />
        public FileStream OpenFile(
            string filename,
            FileAccess access,
            FileShare share
        )
        {
            return OpenFile(
                filename,
                access,
                share,
                DEFAULT_BUFFER_SIZE
            );
        }

        /// <inheritdoc />
        public FileStream OpenFile(
            string filename,
            FileAccess access,
            FileShare share,
            int bufferSize
        )
        {
            return OpenFile(
                filename,
                access,
                share,
                bufferSize,
                DEFAULT_FILE_OPTIONS
            );
        }

        /// <inheritdoc />
        public FileStream OpenFile(
            string filename,
            FileAccess access,
            FileShare share,
            int bufferSize,
            FileOptions options
        )
        {
            var fullPath = ResolvePath(filename);
            return Store(
                new FileStream(
                    fullPath,
                    FileMode.OpenOrCreate,
                    access,
                    share,
                    bufferSize,
                    options
                )
            );
        }

        /// <inheritdoc />
        public bool FileExists(string relativePath)
        {
            return File.Exists(
                ResolvePath(relativePath)
            );
        }

        /// <inheritdoc />
        public bool FolderExists(string relativePath)
        {
            return Directory.Exists(
                ResolvePath(relativePath)
            );
        }

        /// <inheritdoc />
        public bool Exists(string relativePath)
        {
            return FileExists(relativePath) ||
                FolderExists(relativePath);
        }

        /// <inheritdoc />
        public bool Contains(string fullPath)
        {
            return fullPath.Equals(Path, PathComparison) ||
                fullPath.StartsWith(
                    Path + PathSeparator,
                    PathComparison
                );
        }

        private readonly List<FileStream> _fileStreams = new List<FileStream>();

        private FileStream Store(FileStream stream)
        {
            lock (_fileStreams)
            {
                _fileStreams.Add(stream);
            }

            return stream;
        }

        private static readonly StringComparison PathComparison =
            Platform.IsUnixy
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;
    }
}