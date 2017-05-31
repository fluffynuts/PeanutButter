using NUnit.Framework;
using PeanutButter.DuckTyping.Extensions;

namespace PeanutButter.DuckTyping.Tests.Extensions
{
    [TestFixture]
    public class TestDuckTypingCollectionExtensions
    {
        public interface IHasOnlyAnId
        {
            int Id { get; }
        }

        [Test]
        public void DuckAs_GivenOneDuckableItemInCollection_ShouldDuckIt()
        {
            // Arrange
            var src = new[] {
                new {
                    Id = 1
                }
            };

            // Pre-Assert

            // Act
            var result = src.DuckAsArrayOf<IHasOnlyAnId>();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result[0].Id);
        }

        [Test]
        public void DuckAs_GivenTwoDuckableItemInCollection_ShouldDuckIt()
        {
            // Arrange
            var src = new[] {
                new {
                    Id = 1
                },
                new {
                    Id = 2,
                }
            };

            // Pre-Assert

            // Act
            var result = src.DuckAsArrayOf<IHasOnlyAnId>();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result[0].Id);
            Assert.AreEqual(2, result[1].Id);
        }
    }

}
