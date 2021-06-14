using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Describes the contract for an auto-deleting temporary folder
    /// </summary>
    public interface IAutoTempFolder : IDisposable
    {
        /// <summary>
        /// Returns the path to the created folder
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Provides a full path into the temp folder for the provided path parts
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="more"></param>
        /// <returns></returns>
        string ResolvePath(
            string p1,
            params string[] more
        );

        /// <summary>
        /// Creates the desired relative-pathed directory, if it doesn't
        /// exist already
        /// </summary>
        /// <param name="dirname"></param>
        /// <exception cref="NotImplementedException"></exception>
        string CreateDirectory(string dirname);

        /// <summary>
        /// Writes text to the provided relative file path
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="data"></param>
        /// <returns>full path to the file</returns>
        /// <exception cref="NotImplementedException"></exception>
        string WriteFile(
            string filename,
            string data
        );

        /// <summary>
        /// Writes data to the provided relative file path
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="data"></param>
        /// <returns>full path to the file</returns>
        string WriteFile(
            string filename,
            byte[] data
        );

        /// <summary>
        /// Writes data to the provided relative file path
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="data"></param>
        /// <returns>full path to the file</returns>
        string WriteFile(
            string filename,
            Stream data
        );

        /// <summary>
        /// Reads a text file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        string ReadTextFile(string filename);

        /// <summary>
        /// Reads a binary file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        byte[] ReadFile(string filename);

        /// <summary>
        /// Opens the relative path as a file with the desired access
        /// - will dispose the handle when this auto-temp-folder is disposed
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="access"></param>
        /// <returns></returns>
        FileStream OpenFile(string filename, FileAccess access);
    }

    /// <summary>
    /// Provides a mechanism for creating a temporary folder which is automatically
    /// deleted upon disposal.
    /// </summary>
    public class AutoTempFolder : IAutoTempFolder
    {
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
            return string.Join(
                PathSeparator,
                new[] { Path, p1 }.Concat(more)
            );
        }

        private static readonly string PathSeparator
            = System.IO.Path.DirectorySeparatorChar.ToString();

        /// <inheritdoc />
        public string CreateDirectory(string dirname)
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
        public FileStream OpenFile(string filename, FileAccess access)
        {
            var fullPath = ResolvePath(filename);
            return Store(new FileStream(fullPath, FileMode.OpenOrCreate, access));
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
    }
}