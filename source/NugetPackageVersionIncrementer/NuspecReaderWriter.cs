using System;
using System.IO;
using System.Text;

namespace NugetPackageVersionIncrementer
{
    public class NuspecReaderWriter
    {
        private readonly string _path;
        public string NuspecXml { get; set; }

        public NuspecReaderWriter(string path)
        {
            if (!File.Exists(path))
                throw new ArgumentException(path + " not found", nameof(path));
            NuspecXml = Encoding.UTF8.GetString(File.ReadAllBytes(path));
            _path = path;
        }

        public void Rewrite()
        {
            File.WriteAllBytes(_path, Encoding.UTF8.GetBytes(NuspecXml));
        }
    }
}
