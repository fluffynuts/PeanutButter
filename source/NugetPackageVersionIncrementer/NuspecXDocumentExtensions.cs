using System.Xml.Linq;
using System.Xml.XPath;

namespace NugetPackageVersionIncrementer
{
    public static class NuspecXDocumentExtensions
    {
        public static string GetDependencyVersionFor(this XDocument doc, string packageName)
        {
            var el = FindDependencyNodeFor(doc, packageName);
            return el?.Attribute("version")?.Value;
        }

        public static XElement FindDependencyNodeFor(this XDocument doc, string packageName)
        {
            return doc.XPathSelectElement($"/package/metadata/dependencies/group/dependency[@id='{packageName}']")
                ?? doc.XPathSelectElement($"/Project/ItemGroup/PackageReference[Include=[{packageName}]");
        }

        public static void SetDependencyVersionIfExistsFor(this XDocument doc, string packageName, string version)
        {
            var node = doc.FindDependencyNodeFor(packageName);
            node?.Attribute("version")?.SetValue(version);
        }
    }
}