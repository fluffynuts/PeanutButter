using System.Linq;
using System.Xml.Linq;

namespace PeanutButter.XmlUtils
{
    public static class XElementExtensions
    {
        public static string Text(this XElement el)
        {
            if (el == null) return null;
            var textNodes = el.Nodes().OfType<XText>().Where(n => !string.IsNullOrWhiteSpace(n.Value));
            return string.Join("\n", textNodes.Select(n => n.Value));
        }

        public static void ScrubNamespaces(
            this XDocument doc)
        {
            doc.Root.ScrubNamespaces();
        }

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
