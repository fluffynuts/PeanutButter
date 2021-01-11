using NUnit.Framework;
using NExpect;
using static NExpect.Expectations;

namespace PeanutButter.Args.Tests
{
    [TestFixture]
    public class TestArgsParser
    {
        [TestCase("-")]
        [TestCase("--")]
        [Ignore("WIP")]
        public void ShouldParseOneArgumentWithIdenticalNameDashed_(string dashes)
        {
            // Arrange
            
            // Act
            // Assert
        }
    }
}