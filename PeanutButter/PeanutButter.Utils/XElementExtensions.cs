using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PeanutButter.Utils
{
    public static class XElementExtensions
    {
        public static string Text(this XElement el)
        {
            if (el == null) return null;
            var textNodes = el.Nodes().OfType<XText>().Where(n => !String.IsNullOrWhiteSpace(n.Value));
            return String.Join("\n", textNodes.Select(n => n.Value));
        }

    }
}
