using PeanutButter.TestUtils.AspNetCore.Builders;

namespace PeanutButter.TestUtils.AspNetCore.Tests
{
    [TestFixture]
    public class TestActionContextBuilder
    {
        [TestFixture]
        public class DefaultBuild
        {
            [Test]
            public void ShouldHaveHttpContext()
            {
                // Arrange
                // Act
                var result = ActionContextBuilder.BuildDefault();
                // Assert
                Expect(result.HttpContext)
                    .Not.To.Be.Null();
            }

            [Test]
            public void ShouldHaveModelStateDictionary()
            {
                // Arrange
                // Act
                var result = ActionContextBuilder.BuildDefault();
                // Assert
                Expect(result.ModelState)
                    .Not.To.Be.Null();
            }

            [Test]
            public void ShouldHaveActionDescriptor()
            {
                // Arrange
                // Act
                var result = ActionContextBuilder.BuildDefault();
                // Assert
                var actionDescriptor = result.ActionDescriptor;
                Expect(actionDescriptor)
                    .Not.To.Be.Null();
                Expect(actionDescriptor.RouteValues)
                    .To.Be.Empty();
                Expect(actionDescriptor.Properties)
                    .To.Be.Empty();
                Expect(actionDescriptor.AttributeRouteInfo)
                    .To.Be.Null();
            }

            [Test]
            public void ShouldHaveEmptyRouteData()
            {
                // Arrange
                // Act
                var result = ActionContextBuilder.BuildDefault();
                // Assert
                Expect(result.RouteData)
                    .Not.To.Be.Null();
                Expect(result.RouteData)
                    .To.Deep.Equal(RouteDataBuilder.BuildDefault());
            }
        }

        [Test]
        public void ShouldBeAbleToSetRouteDataItem()
        {
            // Arrange
            var key = GetRandomString();
            var value = GetRandomString();
            // Act
            var result = ActionContextBuilder.Create()
                .WithRouteDataValue(key, value)
                .Build();
            // Assert
            Expect(result.RouteData.Values)
                .To.Contain.Key(key)
                .With.Value(value);
        }
    }
}