using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Optimization;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils;

namespace PeanutButter.MVC.Tests
{
    // beware: tests here will reach into the bowels of MVC internal classes
    [TestFixture]
    public class TestScriptBundleFacade
    {
        [Test]
        public void Type_ShouldImplement_IScriptBundle()
        {
            //---------------Set up test pack-------------------
            var sut = typeof (ScriptBundleFacade);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldImplement<IScriptBundle>();

            //---------------Test Result -----------------------
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void Construct_GivenBadName_ShouldThrow(string name)
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ex = Assert.Throws<ArgumentException>(() => new ScriptBundleFacade(name));

            //---------------Test Result -----------------------
            Assert.AreEqual("name", ex.ParamName);
        }

        [Test]
        public void IncludeDirectory_ShouldReturnDirectoryInclusionScriptBundleForThatDirectory()
        {
            //---------------Set up test pack-------------------
            var sut = Create();
            var expectedFolder = "~/" + RandomValueGen.GetRandomString();
            var expectedSearchPattern = "*." + RandomValueGen.GetRandomString(2, 3);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = sut.IncludeDirectory(expectedFolder, expectedSearchPattern);

            //---------------Test Result -----------------------
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ScriptBundle>(result);
            var propValue = result.GetPropertyValue<IEnumerable<object>>("Items");
            var item = propValue.Single();
            var virtualPath = item.GetPropertyValue<string>("VirtualPath");
            var searchPattern = item.GetPropertyValue<string>("SearchPattern");
            var searchSubdirectories = item.GetPropertyValue<bool>("SearchSubdirectories");
            Assert.AreEqual(expectedSearchPattern, searchPattern);
            Assert.AreEqual(expectedFolder, virtualPath);
            Assert.IsFalse(searchSubdirectories);
        }

        [Test]
        public void IncludeDirectory_ShouldAddInputIntoExposedCollectionForTestingPurposes()
        {
            //---------------Set up test pack-------------------
            var sut = Create();
            var expectedFolder = "~/" + RandomValueGen.GetRandomString();
            var expectedSearchPattern = "*." + RandomValueGen.GetRandomString(2, 3);

            //---------------Assert Precondition----------------
            CollectionAssert.IsEmpty(sut.IncludedDirectories);

            //---------------Execute Test ----------------------
            sut.IncludeDirectory(expectedFolder, expectedSearchPattern);

            //---------------Test Result -----------------------
            var item = sut.IncludedDirectories.Single();
            Assert.AreEqual(expectedFolder, item.Path);
            Assert.AreEqual(expectedSearchPattern, item.SearchPattern);
            Assert.IsFalse(item.SearchSubdirectories);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void IncludeDirectory_WithSearchSubDirectoriesArgument_ShouldReturnDirectoryInclusionScriptBundleForThatDirectory(bool expectedSearchSubdirectories)
        {
            //---------------Set up test pack-------------------
            var sut = Create();
            var expectedFolder = "~/" + RandomValueGen.GetRandomString();
            var expectedSearchPattern = "*." + RandomValueGen.GetRandomString(2, 3);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = sut.IncludeDirectory(expectedFolder, expectedSearchPattern, expectedSearchSubdirectories);

            //---------------Test Result -----------------------
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ScriptBundle>(result);
            var propValue = result.GetPropertyValue<IEnumerable<object>>("Items");
            var item = propValue.Single();
            var virtualPath = item.GetPropertyValue<string>("VirtualPath");
            var searchPattern = item.GetPropertyValue<string>("SearchPattern");
            var searchSubdirectories = item.GetPropertyValue<bool>("SearchSubdirectories");
            Assert.AreEqual(expectedSearchPattern, searchPattern);
            Assert.AreEqual(expectedFolder, virtualPath);
            Assert.AreEqual(expectedSearchSubdirectories, searchSubdirectories);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void IncludeDirectory_WithSearchSubdirectoriesArgument_ShouldAddInputIntoExposedCollectionForTestingPurposes(bool expectedSearchSubdirectories)
        {
            //---------------Set up test pack-------------------
            var sut = Create();
            var expectedFolder = "~/" + RandomValueGen.GetRandomString();
            var expectedSearchPattern = "*." + RandomValueGen.GetRandomString(2, 3);

            //---------------Assert Precondition----------------
            CollectionAssert.IsEmpty(sut.IncludedDirectories);

            //---------------Execute Test ----------------------
            sut.IncludeDirectory(expectedFolder, expectedSearchPattern, expectedSearchSubdirectories);

            //---------------Test Result -----------------------
            var item = sut.IncludedDirectories.Single();
            Assert.AreEqual(expectedFolder, item.Path);
            Assert.AreEqual(expectedSearchPattern, item.SearchPattern);
            Assert.AreEqual(expectedSearchSubdirectories, item.SearchSubdirectories);
        }

        [Test]
        public void Include_ShouldReturnValidScriptBundle()
        {
            //---------------Set up test pack-------------------
            var sut = Create();
            var expected = "~/" + RandomValueGen.GetRandomString();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = sut.Include(expected);

            //---------------Test Result -----------------------
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ScriptBundle>(result);
            var propValue = result.GetPropertyValue<IEnumerable<object>>("Items");
            var item = propValue.Single();
            var virtualPath = item.GetPropertyValue<string>("VirtualPath");
            item.GetType().ShouldNotHaveProperty("SearchPattern");
            item.GetType().ShouldNotHaveProperty("SearchSubdirectories");
            Assert.AreEqual(expected, virtualPath);
        }

        private ScriptBundleFacade Create(string name = null)
        {
            return new ScriptBundleFacade(name ?? "~/" + RandomValueGen.GetRandomString());
        }
    }
}
