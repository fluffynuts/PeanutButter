using System.Diagnostics;
using System.Xml.Linq;
using NUnit.Framework;
using PeanutButter.RandomGenerators;

namespace PeanutButter.XmlUtils.Tests
{
    [TestFixture]
    public class TestXElementExtensions
    {
        [Test]
        public void Text_ReturnsSingleTextNodeOfElement()
        {
            // test setup
            var text = RandomValueGen.GetRandomString(10, 20);
            var tag = RandomValueGen.GetRandomAlphaString(10, 20);
            var el = new XElement(tag, new XText(text));
            
            // pre-conditions

            // execute test
            var result = el.Text();

            // test result
            Assert.IsNotNull(result);
            Assert.AreEqual(text, result);
        }

        [Test]
        public void Text_ReturnsMultipleTextNodesSeparatedWithNewlines()
        {
            // test setup
            var t1 = RandomValueGen.GetRandomAlphaString(10, 20);
            var t2 = RandomValueGen.GetRandomAlphaString(10, 20);
            var tag = RandomValueGen.GetRandomAlphaString(8, 20);
            var el = new XElement(tag, new XText(t1), new XText(t2));
            
            // pre-conditions

            // execute test
            var result = el.Text();

            // test result
            Debug.WriteLine(result);
            var parts = result.Split('\n');
            Assert.AreEqual(2, parts.Length);
            Assert.AreEqual(t1, parts[0]);
            Assert.AreEqual(t2, parts[1]);
        }
    }
}
