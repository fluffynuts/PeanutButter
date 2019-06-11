using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using PeanutButter.Utils;

namespace NugetPackageVersionIncrementer
{
    public static class NuspecXDocumentExtensions
    {
        public static string GetDependencyVersionFor(this XDocument doc, string packageName)
        {
            var el = FindDependencyNodesFor(doc, packageName);
            return el.FirstOrDefault()?.Attribute("version")?.Value;
        }

        public static IEnumerable<XElement> FindDependencyNodesFor(this XDocument doc, string packageName)
        {
            return doc.XPathSelectElements(
                $"/package/metadata/dependencies/group/dependency[@id='{packageName}']"
            );
        }

        public static void SetDependencyVersionIfExistsFor(this XDocument doc, string packageName, string version)
        {
            var nodes = doc.FindDependencyNodesFor(packageName);
            nodes.ForEach(node =>
                node.Attribute("version")?.SetValue(version)
            );
        }
    }
}