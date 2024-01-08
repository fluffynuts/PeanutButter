using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Optimization;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils;
// ReSharper disable ObjectCreationAsStatement

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
            Expect(() => new ScriptBundleFacade(name))
                .To.Throw<ArgumentException>()
                .For("name");

            //---------------Test Result -----------------------
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
            Expect(result)
                .To.Be.An.Instance.Of<ScriptBundle>();
            
            var propValue = result.GetPropertyValue<IEnumerable<object>>("Items");
            var item = propValue.Single();
            var virtualPath = item.GetPropertyValue<string>("VirtualPath");
            var searchPattern = item.GetPropertyValue<string>("SearchPattern");
            var searchSubdirectories = item.GetPropertyValue<bool>("SearchSubdirectories");

            Expect(searchPattern)
                .To.Equal(expectedSearchPattern);
            Expect(virtualPath)
                .To.Equal(expectedFolder);
            Expect(searchSubdirectories)
                .To.Be.False();
        }

        [Test]
        public void IncludeDirectory_ShouldAddInputIntoExposedCollectionForTestingPurposes()
        {
            //---------------Set up test pack-------------------
            var sut = Create();
            var expectedFolder = "~/" + RandomValueGen.GetRandomString();
            var expectedSearchPattern = "*." + RandomValueGen.GetRandomString(2, 3);

            //---------------Assert Precondition----------------
            Expect(sut.IncludedDirectories)
                .To.Be.Empty();

            //---------------Execute Test ----------------------
            sut.IncludeDirectory(expectedFolder, expectedSearchPattern);

            //---------------Test Result -----------------------
            var item = sut.IncludedDirectories.Single();

            Expect(item.Path)
                .To.Equal(expectedFolder);
            Expect(item.SearchPattern)
                .To.Equal(expectedSearchPattern);
            Expect(item.SearchSubdirectories)
                .To.Be.False();
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
            Expect(result)
                .To.Be.An.Instance.Of<ScriptBundle>();
            var propValue = result.GetPropertyValue<IEnumerable<object>>("Items");
            var item = propValue.Single();
            var virtualPath = item.GetPropertyValue<string>("VirtualPath");
            var searchPattern = item.GetPropertyValue<string>("SearchPattern");
            var searchSubdirectories = item.GetPropertyValue<bool>("SearchSubdirectories");

            Expect(searchPattern)
                .To.Equal(expectedSearchPattern);
            Expect(virtualPath)
                .To.Equal(expectedFolder);
            Expect(searchSubdirectories)
                .To.Equal(expectedSearchSubdirectories);
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
            Expect(sut.IncludedDirectories)
                .To.Be.Empty();

            //---------------Execute Test ----------------------
            sut.IncludeDirectory(expectedFolder, expectedSearchPattern, expectedSearchSubdirectories);

            //---------------Test Result -----------------------
            var item = sut.IncludedDirectories.Single();
            Expect(item.Path)
                .To.Equal(expectedFolder);
            Expect(item.SearchPattern)
                .To.Equal(expectedSearchPattern);
            Expect(item.SearchSubdirectories)
                .To.Equal(expectedSearchSubdirectories);
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
            Expect(result)
                .To.Be.An.Instance.Of<ScriptBundle>();
            var propValue = result.GetPropertyValue<IEnumerable<object>>("Items");
            var item = propValue.Single();
            var virtualPath = item.GetPropertyValue<string>("VirtualPath");
            item.GetType().ShouldNotHaveProperty("SearchPattern");
            item.GetType().ShouldNotHaveProperty("SearchSubdirectories");
            Expect(virtualPath)
                .To.Equal(expected);
        }

        private ScriptBundleFacade Create(string name = null)
        {
            return new ScriptBundleFacade(name ?? "~/" + RandomValueGen.GetRandomString());
        }
    }
}
