using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.Utils;
// ReSharper disable ObjectCreationAsStatement
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable PossibleNullReferenceException

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
            var sut = Create(TestResources.package1.AsBytes());
            var expected = XDocument.Parse(Encoding.UTF8.GetString(TestResources.package1.AsBytes())).ToString();
            Expect(expected)
                .Not.To.Be.Null();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = sut.NuspecXml;

            //---------------Test Result -----------------------
            Expect(result)
                .To.Equal(expected);
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
            Expect(sut.NuspecXml)
                .To.Equal(before);
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
            Expect(deps)
                .Not.To.Be.Empty();
            var entityDep = deps.First();
            var entityVersion = entityDep.Attribute("version").Value;
            Expect(entityVersion)
                .To.Equal("[6.1.3,]");
            Expect(entityVersion)
                .To.Equal(docBefore.GetDependencyVersionFor("EntityFramework"));
            var expected = "7.0.1";

            //---------------Execute Test ----------------------
            sut.SetExistingPackageDependencyVersion("EntityFramework", "7.0.1");

            //---------------Test Result -----------------------
            Expect(sut.NuspecXml)
                .Not.To.Equal(before);
            var afterDoc = XDocument.Parse(sut.NuspecXml);
            Expect(afterDoc.GetDependencyVersionFor("EntityFramework"))
                .To.Equal(expected);
        }


        private NuspecPackageDependencyHelper Create(byte[] data = null)
        {
            return new NuspecPackageDependencyHelper(Encoding.UTF8.GetString(data ?? TestResources.package1.AsBytes()));
        }
    }
}
