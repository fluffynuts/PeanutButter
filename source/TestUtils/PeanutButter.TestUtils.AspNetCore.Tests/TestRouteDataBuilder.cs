using Microsoft.AspNetCore.Routing;
using PeanutButter.TestUtils.AspNetCore.Builders;

namespace PeanutButter.TestUtils.AspNetCore.Tests;

[TestFixture]
public class TestRouteDataBuilder
{
    [Test]
    public void ShouldBeAbleToSetARouteValue()
    {
        // Arrange
        var key = GetRandomString();
        var value = GetRandomString();
        // Act
        var result = RouteDataBuilder.Create()
            .WithRouteValue(key, value)
            .Build();
        // Assert
        Expect(result.Values)
            .To.Contain.Key(key)
            .With.Value(value);
    }

    [Test]
    public void ShouldBeAbleToClearRouteValues()
    {
        // Arrange
        var key = GetRandomString();
        var value = GetRandomString();
        // Act
        var result = RouteDataBuilder.Create()
            .WithRouteValue(key, value)
            .WithNoRouteValues()
            .Build();
        // Assert
        Expect(result.Values)
            .To.Be.Empty();
    }

    [Test]
    public void ShouldBeAbleToSetADataToken()
    {
        // Arrange
        var key = GetRandomString();
        var value = GetRandomString();
        // Act
        var result = RouteDataBuilder.Create()
            .WithDataToken(key, value)
            .Build();
        // Assert
        Expect(result.DataTokens)
            .To.Contain.Key(key)
            .With.Value(value);
    }

    [Test]
    public void ShouldBeAbleToClearDataTokens()
    {
        // Arrange
        var key = GetRandomString();
        var value = GetRandomString();
        // Act
        var result = RouteDataBuilder.Create()
            .WithDataToken(key, value)
            .WithNoDataTokens()
            .Build();
        // Assert
        Expect(result.DataTokens)
            .To.Be.Empty();
    }

    [Test]
    public void ShouldBeAbleToAddARouter()
    {
        // Arrange
        var expected = Substitute.For<IRouter>();
        // Act
        var result = RouteDataBuilder.Create()
            .WithRouter(expected)
            .Build();
        // Assert
        Expect(result.Routers)
            .To.Contain(expected);
    }

    [Test]
    public void ShouldBeAbleToClearRouters()
    {
        // Arrange
        var unexpected = Substitute.For<IRouter>();
        // Act
        var result = RouteDataBuilder.Create()
            .WithRouter(unexpected)
            .WithNoRouters()
            .Build();
        // Assert
        Expect(result.Routers)
            .To.Be.Empty();
    }
}