using NExpect;
using NUnit.Framework;
using PeanutButter.TestUtils.Entity.Attributes;
using static PeanutButter.RandomGenerators.RandomValueGen;
using static NExpect.Expectations;

namespace PeanutButter.TestUtils.Entity.Tests
{
    [TestFixture]
    public class TestUseSharedTempDbAttribute
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
            Expect(result).To.Equal(expected);
        }
    }
}