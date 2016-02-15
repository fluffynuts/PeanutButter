using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace NugetPackageVersionIncrementer
{
    public interface INuspecUtil
    {
        string Version { get; }
        string OriginalVersion { get; }
        string PackageId { get; }
        string NuspecXml { get; set; }
        void IncrementVersion();
        void SetPackageDependencyVersionIfExists(string packageId, string version);
        void LoadNuspecAt(string nuspecPath);
        void Persist();
    }

    public class NuspecUtil : INuspecUtil
    {
        public string Version { get; private set; }
        public string OriginalVersion { get; private set; }
        public string PackageId { get; private set; }

        public string NuspecXml
        {
            get { return _reader.NuspecXML; }
            set { _reader.NuspecXML = value; }
        }

        private NuspecReaderWriter _reader;

        public void LoadNuspecAt(string nuspecPath)
        {
            ValidatePath(nuspecPath);
            _reader = new NuspecReaderWriter(nuspecPath);
            var incrementer = new NuspecVersionIncrementer(_reader.NuspecXML);
            Version = OriginalVersion = incrementer.Version;
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
            var versionIncrementor = new NuspecVersionIncrementer(_reader.NuspecXML);
            versionIncrementor.IncrementMinorVersion();
            NuspecXml = versionIncrementor.GetUpdatedNuspec();
            Version = versionIncrementor.Version;
        }

        public void SetPackageDependencyVersionIfExists(string packageId, string version)
        {
            var doc = XDocument.Parse(NuspecXml);
            doc.SetDependencyVersionIfExistsFor(packageId, version);
            NuspecXml = doc.ToString();
        }

        public void Persist()
        {
            _reader.Rewrite();
        }
    }
}
