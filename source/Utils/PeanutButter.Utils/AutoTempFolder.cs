using System;
using System.IO;

namespace PeanutButter.Utils
{
    public class AutoTempFolder: IDisposable
    {
        public string Path { get; private set; }
        private object _lock = new object();
        private AutoDeleter _autoDeleter;

        public AutoTempFolder(): this(System.IO.Path.GetTempPath())
        {
        }
        public AutoTempFolder(string baseFolder)
        {
            do
            {
                Path = System.IO.Path.Combine(baseFolder, Guid.NewGuid().ToString());
            } while (Directory.Exists(Path));
            Directory.CreateDirectory(Path);
            _autoDeleter = new AutoDeleter(Path);
        }

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