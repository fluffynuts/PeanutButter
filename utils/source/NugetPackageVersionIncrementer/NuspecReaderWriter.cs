using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NugetPackageVersionIncrementer
{
    public class NuspecReaderWriter
    {
        private string _path;
        public string NuspecXML { get; set; }

        public NuspecReaderWriter(string path)
        {
            if (!File.Exists(path))
                throw new ArgumentException(path + " not found", "path");
            NuspecXML = Encoding.UTF8.GetString(File.ReadAllBytes(path));
            _path = path;
        }

        public void Rewrite()
        {
            File.WriteAllBytes(_path, Encoding.UTF8.GetBytes(NuspecXML));
        }
    }
}
