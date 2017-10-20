using NUnit.Framework;
using PeanutButter.TestUtils.Generic;

namespace PeanutButter.ServiceShell.Tests
{
    [TestFixture]
    public class TestParserFacade
    {
        [Test]
        public void Type_ShouldImplement_IParser()
        {
            //---------------Set up test pack-------------------
            var sut = typeof (ParserFacade);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldImplement<IParser>();

            //---------------Test Result -----------------------
        }

    }
}