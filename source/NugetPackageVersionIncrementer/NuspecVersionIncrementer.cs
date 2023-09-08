using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using PeanutButter.XmlUtils;

// ReSharper disable MemberCanBePrivate.Global

namespace NugetPackageVersionIncrementer
{
    public class NuspecVersionIncrementer
    {
        private readonly string _nuspecXml;
        private readonly string _nuspecPath;
        public string Version { get; set; }
        public string PackageId { get; private set; }
        private XDocument _doc;

        public NuspecVersionIncrementer(
            string nuspecXml,
            string nuspecPath
        )
        {
            if (string.IsNullOrWhiteSpace(nuspecXml))
            {
                throw new ArgumentException(
                    "Nuspec is not valid XML",
                    nameof(nuspecXml)
                );
            }

            _nuspecXml = nuspecXml;
            _nuspecPath = nuspecPath;

            Parse(nuspecXml.Trim());
        }

        private void Parse(string nuspec)
        {
            try
            {
                _doc = XDocument.Parse(nuspec);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    $"{_nuspecPath ?? _nuspecXml} has invalid XML: {ex.Message}",
                    nameof(nuspec)
                );
            }

            GrokVersion();
            GrokPackageId();
        }

        private void GrokPackageId()
        {
            var idNode = _doc.XPathSelectElements("/package/metadata/id").FirstOrDefault();
            if (idNode == null)
            {
                throw new Exception($"No package id node found in {_nuspecPath ?? _nuspecXml}");
            }

            PackageId = idNode.Text();
        }

        private void GrokVersion()
        {
            var node = FindVersionNode();
            if (node == null)
            {
                return;
            }

            Version = node.Text();
        }

        private XElement FindVersionNode()
        {
            return _doc.XPathSelectElements("/package/metadata/version").FirstOrDefault();
        }

        public void IncrementMinorVersion()
        {
            if (string.IsNullOrEmpty(Version))
            {
                Version = "0.0.1";
                return;
            }

            var versionNumbers = GetVersionAsNumbers().ToArray();
            var minor = versionNumbers.Last();
            var preMinor = versionNumbers.Reverse().Skip(1).Reverse();
            var incremented = new List<int>(preMinor)
            {
                minor + 1
            };
            Version = string.Join(".", incremented);
        }

        private IEnumerable<int> GetVersionAsNumbers()
        {
            return Version.Split('.').Select(GetIntFrom);
        }

        private int GetIntFrom(string part)
        {
            int result;
            if (int.TryParse(part, out result))
            {
                return result;
            }

            return 0;
        }

        public string GetUpdatedNuspec()
        {
            var versionNode = FindVersionNode();
            if (versionNode == null)
            {
                throw new Exception($"Unable to find version node in {_nuspecPath ?? _nuspecXml}");
            }

            versionNode.Value = Version;
            return _doc.ToString();
        }
    }
}