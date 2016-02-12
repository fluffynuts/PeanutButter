using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace NugetPackageVersionIncrementer
{
    public class NuspecPackageDependencyHelper
    {
        private XDocument _doc;

        public string NuspecXml { get { return _doc.ToString(); } }

        public NuspecPackageDependencyHelper(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml)) throw new ArgumentException(nameof(xml));
            try
            {
                _doc = XDocument.Parse(xml);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Unable to parse argument as XDocument: " + ex.Message, nameof(xml));
            }
        }

        public void SetMinimumPackageDependencyVersion(string packageId, string minimumVersion)
        {
            var currentVersion = _doc.GetDependencyVersionFor(packageId);
            if (currentVersion == null)
                return;
            var version = new NuspecVersion(currentVersion)
            {
                Minimum = minimumVersion
            };
            _doc.SetDependencyVersionFor(packageId, version.ToString());
        }
    }

    public class NuspecVersion
    {
        public string Minimum { get; set; }
        public string Maximum { get; set; }
        public NuspecVersion(string version)
        {
            if (!version.StartsWith("["))
            {
                Minimum = version;
                Maximum = version;
            }
            var parts = version.Trim('[', ']').Split(',');
            Minimum = parts.First();
            Maximum = parts.Skip(1).FirstOrDefault();
        }

        public override string ToString()
        {
            if (Minimum == Maximum)
                return Minimum;
            return $"[{Minimum},{Maximum}]";
        }

    }

    public static class NuspecXDocumentExtensions
    {
        public static string GetDependencyVersionFor(this XDocument doc, string packageName)
        {
            var el = FindDependencyNodeFor(doc, packageName);
            return el?.Value;
        }

        public static XElement FindDependencyNodeFor(this XDocument doc, string packageName)
        {
            return doc.XPathSelectElement($"/package/metadata/dependencies/group/dependency[@id='{packageName}']");
        }

        public static void SetDependencyVersionFor(this XDocument doc, string packageName, string version)
        {
            var node = doc.FindDependencyNodeFor(packageName);
            if (node == null) return;
            node.Value = version;
        }
    }
}
