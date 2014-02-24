using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.Utils;

namespace PenautButter.Utils.Tests
{
    [TestFixture]
    class TestXElementExtensions
    {
        [Test]
        public void Text_ReturnsSingleTextNodeOfElement()
        {
            // test setup
            var text = RandomValueGen.GetRandomString();
            var tag = RandomValueGen.GetRandomAlphaNumericString();
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
            var t1 = RandomValueGen.GetRandomString();
            var t2 = RandomValueGen.GetRandomString();
            var tag = RandomValueGen.GetRandomAlphaNumericString();
            var el = new XElement(tag, new XText(t1), new XText(t2));
            
            // pre-conditions

            // execute test
            var result = el.Text();

            // test result
            var parts = result.Split('\n');
            Assert.AreEqual(2, parts.Length);
            Assert.AreEqual(t1, parts[0]);
            Assert.AreEqual(t2, parts[1]);
        }
    }
}
