using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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
            _doc.SetDependencyVersionIfExistsFor(packageId, version);
        }
    }
}
