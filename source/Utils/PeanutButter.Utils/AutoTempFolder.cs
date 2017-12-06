using System;
using System.IO;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Provides a mechanism for creating a temporary folder which is automatically
    /// deleted upon disposal.
    /// </summary>
    public class AutoTempFolder: IDisposable
    {
        /// <summary>
        /// Returns the path to the created folder
        /// </summary>
        public string Path { get; private set; }
        private readonly object _lock = new object();
        private AutoDeleter _autoDeleter;

        /// <summary>
        /// Default constructor: uses the operating system method to
        /// get a temporary path to use for the folder
        /// </summary>
        public AutoTempFolder(): this(System.IO.Path.GetTempPath())
        {
        }

        /// <summary>
        /// Constructs a new AutoTempFolder with the temporary folder
        /// hosued under the provided baseFolder
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
                _autoDeleter?.Dispose();
                _autoDeleter = null;
            }
        }

    }
}