using NUnit.Framework;

namespace PeanutButter.TestUtils.Entity.Tests
{
    [TestFixture]
    public class TestSharedTempDbFeaterRequiresAssemblyAttributeException
        : AssertionHelper

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
            Expect(sut.Message, Does.Contain("class attribute"));
            Expect(sut.Message, Does.Contain("requires that"));
            Expect(sut.Message, Does.Contain("AllowSharedTempDbInstances"));
            Expect(sut.Message, Does.Contain("[assembly: PeanutButter.TestUtils.Entity.Attributes.AllowSharedTempDbInstances]"));
        }

    }
}