using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using PeanutButter.TestUtils.Generic;

namespace PeanutButter.MVC.Tests
{
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
            var ex = Assert.Throws<ArgumentException>(() => Create(name));

            //---------------Test Result -----------------------
            Assert.AreEqual("name", ex.ParamName);
        }

        [Test]
        [Ignore("WIP: pick up from here for trying to up coverage on this assembly")]
        public void IncludeDirectory_ShouldReturnBundleForThatDirectory()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------

            //---------------Test Result -----------------------
            Assert.Fail("Test Not Yet Implemented");
        }



        private ScriptBundleFacade Create(string name)
        {
            return new ScriptBundleFacade(name);
        }
    }
}
