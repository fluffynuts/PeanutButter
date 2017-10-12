using NUnit.Framework;
using NExpect;
using static NExpect.Expectations;

namespace PeanutButter.TestUtils.Entity.Tests
{
    [TestFixture]
    public class TestSharedTempDbFeaterRequiresAssemblyAttributeException
    {
        [Test]
        public void Construct_ShouldSetHelpfulMessage()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var sut = new SharedTempDbFeatureRequiresAssemblyAttributeException(
                GetType()
            );

            //--------------- Assert -----------------------
            Expect(sut.Message)
                .To.Contain("class attribute")
                .Then("requires that")
                .Then("AllowSharedTempDbInstances")
                .Then("[assembly: PeanutButter.TestUtils.Entity.Attributes.AllowSharedTempDbInstances]");
        }

    }
}