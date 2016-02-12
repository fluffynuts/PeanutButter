using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace NugetPackageVersionIncrementer
{
    public class NuspecUtil
    {
        public string Version { get { return _versionIncrementor.Version; } }
        public string PackageId { get; private set; }
        public string NuspecXml { get; private set; }
        private NuspecVersionIncrementer _versionIncrementor;
        private NuspecReaderWriter _reader;
        internal NuspecVersionIncrementer NuspecVersionIncrementer { get { return _versionIncrementor; } }

        public NuspecUtil(string nuspecPath)
        {
            ValidatePath(nuspecPath);
            _reader = new NuspecReaderWriter(nuspecPath);
            _versionIncrementor = new NuspecVersionIncrementer(_reader.NuspecXML);
            PackageId = GrokPackageIdFrom(_reader.NuspecXML);
        }

        private string GrokPackageIdFrom(string nuspecXml)
        {
            var doc = XDocument.Parse(nuspecXml);
            var el = doc.XPathSelectElements("/package/metadata/id").FirstOrDefault();
            return el?.Value;
        }

        private static void ValidatePath(string nuspecPath)
        {
            if (string.IsNullOrWhiteSpace(nuspecPath)) throw new ArgumentException(nameof(nuspecPath));
            if (!File.Exists(nuspecPath)) throw new FileNotFoundException(nuspecPath + " not found");
        }

        public void IncrementVersion()
        {
            _versionIncrementor.IncrementMinorVersion();
            NuspecXml = _versionIncrementor.GetUpdatedNuspec();
        }
    }
}
