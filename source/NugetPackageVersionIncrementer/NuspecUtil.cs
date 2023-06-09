using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using PeanutButter.Utils;

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
        IEnumerable<string> FindTargetedFrameworks();
        void EnsureSameDependencyGroupForAllTargetFrameworks();
    }

    public class NuspecUtil : INuspecUtil
    {
        public string Version { get; private set; }
        public string OriginalVersion { get; private set; }
        public string PackageId { get; private set; }

        public string NuspecXml
        {
            get { return _reader.NuspecXml; }
            set { _reader.NuspecXml = value; }
        }

        private NuspecReaderWriter _reader;

        public void LoadNuspecAt(string nuspecPath)
        {
            ValidatePath(nuspecPath);
            _reader = new NuspecReaderWriter(nuspecPath);
            var incrementer = new NuspecVersionIncrementer(_reader.NuspecXml);
            Version = OriginalVersion = incrementer.Version;
            PackageId = GrokPackageIdFrom(_reader.Document);
        }

        private string GrokPackageIdFrom(XDocument doc)
        {
            var el = doc.XPathSelectElements("/package/metadata/id").FirstOrDefault();
            return el?.Value;
        }

        private static void ValidatePath(string nuspecPath)
        {
            if (string.IsNullOrWhiteSpace(nuspecPath))
            {
                throw new ArgumentException(
                    "Invalid nuspec path provided",
                    nameof(nuspecPath)
                );
            }

            if (!File.Exists(nuspecPath))
            {
                throw new FileNotFoundException($"{nuspecPath} not found");
            }
        }

        public void IncrementVersion()
        {
            var versionIncrementer = new NuspecVersionIncrementer(_reader.NuspecXml);
            versionIncrementer.IncrementMinorVersion();
            NuspecXml = versionIncrementer.GetUpdatedNuspec();
            Version = versionIncrementer.Version;
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

        public IEnumerable<string> FindTargetedFrameworks()
        {
            return _reader.Document.XPathSelectElements(
                    "/package/files/file"
                )
                .Select(n => n.Attribute("target")?.Value)
                .Where(t => !t.IsNullOrWhiteSpace() && t.StartsWith("lib"))
                .Select(t => t.RegexReplace("^lib/", ""))
                .Distinct();
        }

        public void EnsureSameDependencyGroupForAllTargetFrameworks()
        {
            var allDependencies = _reader.Document.XPathSelectElements(
                "/package/metadata/dependencies/group/dependency"
            ).ToArray();
            var parent = _reader.Document.XPathSelectElement(
                "/package/metadata/dependencies"
            );
            if (!allDependencies.Any() || parent == null)
            {
                return;
            }

            var frameworks = FindTargetedFrameworks();
            var deps = allDependencies
                .Select(
                    node =>
                        new
                        {
                            id = node.Attribute("id")?.Value,
                            version = node.Attribute("version")?.Value
                        }
                )
                .Where(
                    dep => !dep.id.IsNullOrWhiteSpace() &&
                        !dep.version.IsNullOrWhiteSpace()
                )
                .Distinct();
            var parents = allDependencies.Select(dep => dep.Parent)
                .Distinct()
                .ToArray();
            parents.ForEach(dep => dep.Remove());

            frameworks.ForEach(
                framework =>
                {
                    var group = new XElement("group", new XAttribute("targetFramework", framework));
                    deps.ForEach(
                        dep =>
                        {
                            group.Add(
                                new XElement(
                                    "dependency",
                                    new XAttribute("id", dep.id),
                                    new XAttribute("version", dep.version)
                                )
                            );
                        }
                    );
                    parent.Add(group);
                }
            );
        }
    }
}