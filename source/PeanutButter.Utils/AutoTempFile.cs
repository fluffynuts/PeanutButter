using System;
using System.IO;
using System.Text;

namespace PeanutButter.Utils
{
    public class AutoTempFile: IDisposable
    {
        public string FileName => _tempFile;
        public byte[] BinaryData
        {
            get { return File.ReadAllBytes(_tempFile); }
            set { File.WriteAllBytes(_tempFile, value); }
        }

        public string StringData
        {
            get { return Encoding.UTF8.GetString(File.ReadAllBytes(_tempFile)); }
            set { File.WriteAllBytes(_tempFile, Encoding.UTF8.GetBytes(value)); }
        }

        private readonly string _tempFile;
        private AutoDeleter _actual;
        private object _lock = new object();

        public AutoTempFile(byte[] data = null)
        {
            _tempFile = Path.GetTempFileName();
            _actual = new AutoDeleter(_tempFile);
            if (data == null)
                return;
            File.WriteAllBytes(_tempFile, data);
        }
        public AutoTempFile(string data): this(Encoding.UTF8.GetBytes(data))
        {
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