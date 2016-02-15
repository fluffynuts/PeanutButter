using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using _Path = System.IO.Path;

namespace PeanutButter.Utils
{
    public class AutoTempFile: IDisposable
    {
        public string Path => _tempFile;
        public byte[] BinaryData
        {
            get { return File.ReadAllBytes(_tempFile); }
            set { File.WriteAllBytes(_tempFile, value); }
        }

        public string StringData
        {
            get { return Encoding.UTF8.GetString(File.ReadAllBytes(_tempFile)); }
            set { File.WriteAllBytes(_tempFile, Encoding.UTF8.GetBytes(value ?? string.Empty)); }
        }

        private string _tempFile;
        private AutoDeleter _actual;
        private object _lock = new object();

        public AutoTempFile(): this(_Path.GetTempPath(), null, (byte[])null)
        {
        }

        public AutoTempFile(byte[] data): this(_Path.GetTempPath(), null, data)
        {
        }

        public AutoTempFile(string data): this(_Path.GetTempPath(), null, Encoding.UTF8.GetBytes(data))
        {
        }

        public AutoTempFile(string baseFolder, string data): this(baseFolder, null, Encoding.UTF8.GetBytes(data ?? string.Empty))
        {
        }

        public AutoTempFile(string baseFolder, byte[] data): this(baseFolder, null, data)
        {
        }

        public AutoTempFile(string baseFolder, string fileName, byte[] data)
        {
            SetTempFileNameWith(baseFolder, fileName);
            _actual = new AutoDeleter(_tempFile);
            baseFolder = _Path.GetDirectoryName(_tempFile) ?? string.Empty;
            if (!Directory.Exists(baseFolder))
                Directory.CreateDirectory(baseFolder);
            File.WriteAllBytes(_tempFile, data ?? new byte[] { });
        }

        private List<Func<string, string, string>> _fileNameStrategies = new List<Func<string, string, string>>()
        {
            FileNameWhenFolderAndFileNotSpecified,
            FileNameWhenFolderSpecifiedAndFileNotSpecified,
            FileNameWhenFolderNotSpecifiedAndFileIsSpecified,
            _Path.Combine
        };

        private static string FileNameWhenFolderNotSpecifiedAndFileIsSpecified(string folder, string file)
        {
            return folder == null && file != null
                    ? _Path.Combine(_Path.GetTempPath(), file)
                    : null;
        }

        private static string FileNameWhenFolderSpecifiedAndFileNotSpecified(string folder, string file)
        {
            return folder != null && file == null
                    ? GetNewFileNameUnder(folder)
                    : null;
        }

        private static string GetNewFileNameUnder(string folder)
        {
            string fileName;
            do
            {
                var tempFile = GetFileNameOfNewTempFile();
                fileName = _Path.Combine(folder, tempFile);
            } while (File.Exists(fileName));
            return fileName;
        }

        private static string GetFileNameOfNewTempFile()
        {
            var tempFileName = _Path.GetTempFileName();
            File.Delete(tempFileName);
            return _Path.GetFileName(tempFileName);
        }

        private static string FileNameWhenFolderAndFileNotSpecified(string folder, string file)
        {
            return folder == null && file == null
                    ? _Path.GetTempFileName()
                    : null;
        }

        private void SetTempFileNameWith(string baseFolder, string fileName)
        {
            _tempFile = _fileNameStrategies
                            .Select(strategy => strategy(baseFolder, fileName))
                            .First(path => path != null);
        }

        public void Dispose()
        {
            lock(_lock)
            {
                _actual?.Dispose();
                _actual = null;
            }
        }
    }
}