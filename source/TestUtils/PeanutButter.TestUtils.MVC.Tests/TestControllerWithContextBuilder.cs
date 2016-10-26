using System.Web.Mvc;
using NUnit.Framework;
using PeanutButter.TestUtils.MVC.Builders;

namespace PeanutButter.TestUtils.MVC.Tests
{
    [TestFixture]
    public class TestControllerWithContextBuilder: AssertionHelper
    {
        [Test]
        public void Build_Default_ShouldBuild()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => 
                ControllerWithContextBuilder<SomeController>.Create().Build(),
                Throws.Nothing);

            //--------------- Assert -----------------------
        }

    }

    public class SomeController: Controller
    {
    }
}
