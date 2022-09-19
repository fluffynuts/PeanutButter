using NUnit.Framework;
using NExpect;
using PeanutButter.TestUtils.AspNetCore.Builders;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TestUtils.AspNetCore.Tests
{
    [TestFixture]
    public class TestViewDataDictionaryBuilder
    {
        [TestFixture]
        public class BuildDefault
        {
            [Test]
            public void ShouldBuildAValidViewData()
            {
                // Arrange
                // Act
                var result = ViewDataDictionaryBuilder.BuildDefault();
                // Assert
                Expect(result)
                    .Not.To.Be.Null();
                Expect(result.Model)
                    .Not.To.Be.Null();
            }
        }

        [Test]
        public void ShouldBeAbleToSetModel()
        {
            // Arrange
            var model = new { Id = 1 };
            // Act
            var result = ViewDataDictionaryBuilder.Create()
                .WithModel(model)
                .Build();
            // Assert
            Expect(result.Model)
                .To.Be(model);
        }
    }
}