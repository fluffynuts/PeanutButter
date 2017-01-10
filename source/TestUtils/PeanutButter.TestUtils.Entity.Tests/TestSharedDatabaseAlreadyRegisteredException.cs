using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TestUtils.Entity.Tests
{
    [TestFixture]
    public class TestSharedDatabaseAlreadyRegisteredException
        : AssertionHelper
    {
        [Test]
        public void Construct_ShouldSetMessage()
        {
            //--------------- Arrange -------------------
            var name = GetRandomString(5);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var sut = new SharedDatabaseAlreadyRegisteredException(name);

            //--------------- Assert -----------------------
            Expect(sut.Message, Does.Contain(name + " is already registered"));
        }
    }
}
