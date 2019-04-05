using NUnit.Framework;
using PeanutButter.TempDb.MySql;
using NExpect;
using PeanutButter.TempDb;
using static NExpect.Expectations;

namespace Tests
{
    public class TestTempDbMySql
    {
        [Test]
        public void ShouldBeAbleToInitialize()
        {
            // Arrange
            // Act
            Expect(() => new TempDBMySql())
                .To.Throw<FatalTempDbInitializationException>();
        }
    }
}