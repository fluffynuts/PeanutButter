using NUnit.Framework;
using PeanutButter.TestUtils.Entity.Attributes;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TestUtils.Entity.Tests
{
    [TestFixture]
    public class TestUseSharedTempDbAttribute: AssertionHelper
    {
        [Test]
        public void Construct_ShouldCopyNameAttribute()
        {
            //--------------- Arrange -------------------
            var expected = GetRandomString(5, 10);
            var sut = new UseSharedTempDbAttribute(expected);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.Name;

            //--------------- Assert -----------------------
            Expect(result, Is.EqualTo(expected));
        }
    }
}