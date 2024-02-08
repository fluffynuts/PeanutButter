using System.Reflection;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.AspNetCore.Builders;

namespace PeanutButter.TestUtils.AspNetCore.Tests;

[TestFixture]
public class TestControllerActionDescriptorBuilder
{
    [TestFixture]
    public class BuildDefault
    {
        [Test]
        public void ShouldProduceEmptyDescriptor()
        {
            // Arrange
            // Act
            var result = ControllerActionDescriptorBuilder.BuildDefault();
            // Assert
            Expect(result)
                .Not.To.Be.Null();
            Expect(result.ControllerTypeInfo)
                .To.Be.Null();
            Expect(result.ControllerName)
                .To.Be.Null();
            Expect(result.ActionName)
                .To.Be.Null();
            Expect(result.MethodInfo)
                .To.Be.Null();
        }
    }

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
        Expect(result.ControllerName)
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
        Expect(result.ActionName)
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
        Expect(result.MethodInfo)
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
        Expect(result.ControllerTypeInfo)
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
        Expect(result.ControllerTypeInfo)
            .To.Be(expected);
    }
}