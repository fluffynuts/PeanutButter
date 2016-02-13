using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.Utils;

namespace NugetPackageVersionIncrementer.Tests
{
    [TestFixture]
    public class TestNuspecPackageDependencyHelper
    {
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\r\n")]
        public void Construct_GivenInvalidXML_ShouldBarf(string xml)
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<ArgumentException>(() => new NuspecPackageDependencyHelper(xml));

            //---------------Test Result -----------------------
        }

        [Test]
        public void NuspecXml_ShouldReturnDocumentAsString()
        {
            //---------------Set up test pack-------------------
            var sut = Create(TestResources.package1);
            var expected = XDocument.Parse(Encoding.UTF8.GetString(TestResources.package1)).ToString();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = sut.NuspecXml;

            //---------------Test Result -----------------------
            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);
        }


        [Test]
        public void SetExistingPackageDependencyVersion_WhenDependencyNotFound_ShouldNotChangeDocument()
        {
            //---------------Set up test pack-------------------
            var sut = Create();
            var before = sut.NuspecXml;

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.SetExistingPackageDependencyVersion(RandomValueGen.GetRandomString(10, 20), RandomValueGen.GetRandomString());

            //---------------Test Result -----------------------
            Assert.AreEqual(before, sut.NuspecXml);
        }

        [Test]
        public void SetExistingPackageDependencyVersion_GivenKnownPackageId_ShouldUpdateDocument()
        {
            //---------------Set up test pack-------------------
            var sut = Create();
            var before = sut.NuspecXml;
            var docBefore = XDocument.Parse(before);

            //---------------Assert Precondition----------------
            var deps = docBefore.XPathSelectElements("/package/metadata/dependencies/group/dependency[@id='EntityFramework']");
            Assert.IsNotNull(deps);
            CollectionAssert.IsNotEmpty(deps);
            var entityDep = deps.First();
            var entityVersion = entityDep.Attribute("version").Value;
            Assert.AreEqual("[6.1.3,]", entityVersion);
            Assert.AreEqual(entityVersion, docBefore.GetDependencyVersionFor("EntityFramework"));
            var expected = "7.0.1";

            //---------------Execute Test ----------------------
            sut.SetExistingPackageDependencyVersion("EntityFramework", "7.0.1");

            //---------------Test Result -----------------------
            Assert.AreNotEqual(before, sut.NuspecXml);
            var afterDoc = XDocument.Parse(sut.NuspecXml);
            Assert.AreEqual(expected, afterDoc.GetDependencyVersionFor("EntityFramework"));
        }


        private NuspecPackageDependencyHelper Create(byte[] data = null)
        {
            return new NuspecPackageDependencyHelper(Encoding.UTF8.GetString(data ?? TestResources.package1));
        }
    }
}
