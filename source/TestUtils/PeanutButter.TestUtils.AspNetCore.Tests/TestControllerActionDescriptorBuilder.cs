using System.Reflection;
using NExpect;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.AspNetCore.Builders;

namespace PeanutButter.TestUtils.AspNetCore.Tests;

[TestFixture]
public class TestControllerActionDescriptorBuilder
{
    [Test]
    public void ShouldBeAbleToSetControllerName()
    {
        // Arrange
        var expected = RandomValueGen.GetRandomString();
        // Act
        var result = ControllerActionDescriptorBuilder.Create()
            .WithControllerName(expected)
            .Build();
        // Assert
        Expectations.Expect(result.ControllerName)
            .To.Equal(expected);
    }

    [Test]
    public void ShouldBeAbleToSetActionName()
    {
        // Arrange
        var expected = RandomValueGen.GetRandomString();
        // Act
        var result = ControllerActionDescriptorBuilder.Create()
            .WithActionName(expected)
            .Build();
        // Assert
        Expectations.Expect(result.ActionName)
            .To.Equal(expected);
    }

    [Test]
    public void ShouldBeAbleToSetMethodInfo()
    {
        // Arrange
        var expected = typeof(TestRouteDataBuilder)
            .GetMethod(nameof(ShouldBeAbleToSetMethodInfo));
        // Act
        var result = ControllerActionDescriptorBuilder.Create()
            .WithMethodInfo(expected)
            .Build();
        // Assert
        Expectations.Expect(result.MethodInfo)
            .To.Be(expected);
    }

    [Test]
    public void ShouldBeAbleToSetControllerTypeInfoFromType()
    {
        // Arrange
        var expected = typeof(TestControllerActionDescriptorBuilder);
        // Act
        var result = ControllerActionDescriptorBuilder.Create()
            .WithControllerType(expected)
            .Build();
        // Assert
        Expectations.Expect(result.ControllerTypeInfo)
            .To.Be(expected);
    }

    [Test]
    public void ShouldBeAbleToSetControllerTypeInfoFromTypeInfo()
    {
        // Arrange
        var expected = typeof(TestControllerActionDescriptorBuilder)
            .GetTypeInfo();
        // Act
        var result = ControllerActionDescriptorBuilder.Create()
            .WithControllerType(expected)
            .Build();
        // Assert
        Expectations.Expect(result.ControllerTypeInfo)
            .To.Be(expected);
    }
}