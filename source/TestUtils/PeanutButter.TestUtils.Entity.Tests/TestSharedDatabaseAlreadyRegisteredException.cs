using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using static NExpect.Expectations;

namespace PeanutButter.TestUtils.Entity.Tests
{
    [TestFixture]
    public class TestSharedDatabaseAlreadyRegisteredException
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
            Expect(sut.Message)
                .To.Contain(name)
                .Then(" is already registered");
        }
    }
}
