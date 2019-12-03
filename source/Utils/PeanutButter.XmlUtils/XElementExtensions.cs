using System.Linq;
using System.Xml.Linq;

namespace PeanutButter.XmlUtils
{
    /// <summary>
    /// Adds some convenience extensions to XElement and XDocument instances
    /// </summary>
    public static class XElementExtensions
    {
        /// <summary>
        /// Returns all text nodes concatenated together for an XElement
        ///  as plain ol' text
        /// </summary>
        /// <param name="el"></param>
        /// <returns></returns>
        public static string Text(this XElement el)
        {
            if (el == null)
            {
                return null;
            }

            var textNodes = el.Nodes()
                .OfType<XText>()
                .Where(n => !string.IsNullOrWhiteSpace(n.Value));
            return string.Join("\n", textNodes.Select(n => n.Value));
        }

        /// <summary>
        /// Scrubs namespaces from a document
        /// - useful when you just want to deal with xml, and
        ///   namespaces are making that a nuisance
        /// </summary>
        /// <param name="doc"></param>
        public static void ScrubNamespaces(
            this XDocument doc)
        {
            doc.Root.ScrubNamespaces();
        }

        /// <summary>
        /// Recursively scrubs namespaces from an element
        /// - useful when you just want to deal with xml, and
        ///   namespaces are making that a nuisance
        /// </summary>
        /// <param name="el"></param>
        public static void ScrubNamespaces(
            this XElement el)
        {
            el.Name = el.Name.LocalName;
            el.Attribute("xmlns")?.Remove();
            foreach (var descendant in el.Descendants()
                .Where(d => !string.IsNullOrEmpty(d.Name.NamespaceName)))
            {
                descendant.ScrubNamespaces();
            }
        }
    }
}
