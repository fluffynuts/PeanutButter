using System;
using System.Collections.Generic;
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

        public void SetExistingPackageDependencyVersion(string packageId, string version)
        {
            if (_doc.GetDependencyVersionFor(packageId) == null)
                return;
            _doc.SetDependencyVersionFor(packageId, version);
        }
    }

    public static class NuspecXDocumentExtensions
    {
        public static string GetDependencyVersionFor(this XDocument doc, string packageName)
        {
            var el = FindDependencyNodeFor(doc, packageName);
            return el?.Attribute("version")?.Value;
        }

        public static XElement FindDependencyNodeFor(this XDocument doc, string packageName)
        {
            return doc.XPathSelectElement($"/package/metadata/dependencies/group/dependency[@id='{packageName}']");
        }

        public static void SetDependencyVersionFor(this XDocument doc, string packageName, string version)
        {
            var node = doc.FindDependencyNodeFor(packageName);
            node?.Attribute("version")?.SetValue(version);
        }
    }
}
