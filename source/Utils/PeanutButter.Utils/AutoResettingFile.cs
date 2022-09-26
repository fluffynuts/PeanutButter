using System;
using System.Collections.Generic;
using System.IO;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ConvertToConstant.Global

namespace PeanutButter.Utils
{
    /// <summary>
    /// The types of storage supported by AutoResettingFile
    /// </summary>
    public enum StorageTypes
    {
        /// <summary>
        /// Automatically select storage based on file size
        /// </summary>
        Auto,

        /// <summary>
        /// Store in memory
        /// </summary>
        InMemory,

        /// <summary>
        /// Store on disk
        /// </summary>
        OnDisk
    }

    /// <summary>
    /// Facilitates easily reverting a file to a prior state
    /// via the IDisposable pattern
    /// </summary>
    public class AutoResettingFile : IDisposable
    {
        /// <summary>
        /// The max file size to store in memory when using Automatic storage
        /// You may modify this at runtime, but it defaults to 10mb
        /// </summary>
        public static readonly int MaxInMemoryFileSizeInBytes = 1024 * 1024 * 10;

        private IDisposable _storage;

        /// <summary>
        /// Create a new AutoResettingFile for the provided
        /// path using automatic storage
        /// </summary>
        /// <param name="path"></param>
        public AutoResettingFile(string path)
            : this(path, StorageTypes.Auto)
        {
        }

        /// <summary>
        /// Create a new AutoResettingFile for the provided path
        /// using the requested storage
        /// </summary>
        /// <param name="path"></param>
        /// <param name="storageType"></param>
        public AutoResettingFile(
            string path,
            StorageTypes storageType
        )
        {
            _storage = ResolveStorage(storageType, path);
        }

        private IDisposable ResolveStorage(StorageTypes storageType, string path)
        {
            if (storageType is StorageTypes.Auto)
            {
                var info = new FileInfo(path);
                storageType = info.Length > MaxInMemoryFileSizeInBytes
                    ? StorageTypes.OnDisk
                    : StorageTypes.InMemory;
            }

            if (!StorageFactories.TryGetValue(storageType, out var factory))
            {
                throw new NotSupportedException($"StorageType {storageType} is not supported");
            }

            return factory(path);
        }

        private static readonly Dictionary<StorageTypes, Func<string, IDisposable>> StorageFactories = new()
        {
            [StorageTypes.Auto] = _ =>
                throw new InvalidOperationException($"Cannot create storage type for {nameof(StorageTypes.Auto)}"),
            [StorageTypes.InMemory] = path => new InMemoryStorage(path),
            [StorageTypes.OnDisk] = path => new OnDiskStorage(path)
        };

        /// <summary>
        /// Reverts the file back to its original state
        /// </summary>
        public void Dispose()
        {
            _storage?.Dispose();
            _storage = null;
        }

        private class InMemoryStorage : IDisposable
        {
            private string _path;
            private readonly byte[] _data;

            public InMemoryStorage(string path)
            {
                _path = path;
                _data = File.ReadAllBytes(_path);
            }

            public void Dispose()
            {
                if (_path is null)
                {
                    return;
                }

                File.WriteAllBytes(_path, _data);
                _path = null;
            }
        }

        private class OnDiskStorage : IDisposable
        {
            private readonly string _path;
            private AutoTempFile _tempFile;

            public OnDiskStorage(string path)
            {
                _path = path;
                Store();
            }

            private void Store()
            {
                _tempFile?.Dispose();
                _tempFile = new AutoTempFile();
                File.Copy(_path, _tempFile.Path, overwrite: true);
            }

            public void Dispose()
            {
                Revert();
                _tempFile.Dispose();
                _tempFile = null;
            }

            public void Revert()
            {
                if (_tempFile is null)
                {
                    throw new InvalidOperationException(
                        "No temp file stored - has this been disposed already?"
                    );
                }

                File.Copy(_tempFile.Path, _path, overwrite: true);
            }
        }
    }
}