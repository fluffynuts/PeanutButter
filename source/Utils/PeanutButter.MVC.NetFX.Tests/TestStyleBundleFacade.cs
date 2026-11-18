using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Optimization;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils;
// ReSharper disable ObjectCreationAsStatement
// ReSharper disable PossibleMultipleEnumeration

namespace PeanutButter.MVC.Tests
{
    [TestFixture]
    public class TestStyleBundleFacade
    {
        [Test]
        public void Type_ShouldImplement_IStyleBundle()
        {
            //---------------Set up test pack-------------------
            var sut = typeof (StyleBundleFacade);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldImplement<IStyleBundle>();

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
            var ex = Assert.Throws<ArgumentException>(() => new StyleBundleFacade(name));
            Expect(() => new StyleBundleFacade(name))
                .To.Throw<ArgumentException>()
                .For("name");
            //---------------Test Result -----------------------
        }

        [Test]
        public void Include_GivenOneVirtualPath_ShouldReturnStyleBundleWithThatPath()
        {
            //---------------Set up test pack-------------------
            var sut = Create();
            var expected = "~/" + RandomValueGen.GetRandomString();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = sut.Include(expected) as StyleBundle;

            //---------------Test Result -----------------------
            Expect(result)
                .Not.To.Be.Null();
            var items = result.GetPropertyValue<IEnumerable<object>>("Items");
            var item = items.Single();
            var path = item.GetPropertyValue<string>("VirtualPath");
            Expect(path)
                .To.Equal(expected);
        }

        [Test]
        public void Include_GivenMoreThanOneVirtualPath_ShouldReturnStyleBundleWithThatPath()
        {
            //---------------Set up test pack-------------------
            var sut = Create();
            var expected1 = "~/" + RandomValueGen.GetRandomString();
            var expected2 = "~/" + RandomValueGen.GetRandomString();
            var expected3 = "~/" + RandomValueGen.GetRandomString();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = sut.Include(expected1, expected2, expected3) as StyleBundle;

            //---------------Test Result -----------------------
            Expect(result)
                .Not.To.Be.Null();
            var items = result.GetPropertyValue<IEnumerable<object>>("Items");
            Expect(items)
                .To.Contain.Only(3).Items();
            Expect(items.First().GetPropertyValue<string>("VirtualPath"))
                .To.Equal(expected1);
            Expect(items.Second().GetPropertyValue<string>("VirtualPath"))
                .To.Equal(expected2);
            Expect(items.Third().GetPropertyValue<string>("VirtualPath"))
                .To.Equal(expected3);

        }

        private StyleBundleFacade Create(string name = null)
        {
            return new StyleBundleFacade(name ?? "~/" + RandomValueGen.GetRandomString());
        }
    }
}