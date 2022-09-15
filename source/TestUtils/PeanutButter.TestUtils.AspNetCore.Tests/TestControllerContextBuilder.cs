using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using NExpect;
using NSubstitute;
using PeanutButter.TestUtils.AspNetCore.Builders;
using PeanutButter.Utils;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;
using static NExpect.AspNetCoreExpectations;

namespace PeanutButter.TestUtils.AspNetCore.Tests;

[TestFixture]
public class TestControllerContextBuilder
{
    [Test]
    public void ShouldBeAbleToSetTheIdentityOnTheHttpContext()
    {
        // Arrange
        var name = GetRandomString();
        var expected = Substitute.For<IIdentity>()
            .With(o => o.Name.Returns(name));
        // Act
        var result = ControllerContextBuilder.Create()
            .WithIdentity(expected)
            .Build();
        // Assert
        Expect(result.HttpContext.User.Identities)
            .To.Contain.Exactly(1)
            .Matched.By(o =>
                o.Name == expected.Name
            );
    }

    [Test]
    public void ShouldBeAbleToSetUser()
    {
        // Arrange
        var expected = new ClaimsPrincipal();
        // Act
        var result = ControllerContextBuilder.Create()
            .WithUser(expected)
            .Build();
        // Assert
        Expect(result.HttpContext.User)
            .To.Be(expected);
    }

    [Test]
    public void ShouldFacilitateArbitraryHttpContextMutations()
    {
        // Arrange
        var key = GetRandomString();
        var value = GetRandomString();
        // Act
        var result = ControllerContextBuilder.Create()
            .WithHttpContextMutator(
                o => o.Request.Headers[key] = value
            )
            .Build();
        // Assert
        Expect(result.HttpContext.Request.Headers)
            .To.Contain.Key(key)
            .With.Value(value);
    }

    [Test]
    public void ShouldBeAbleToAssociateAController()
    {
        // Arrange
        var controller = new MyController();
        
        // Act
        var result = ControllerContextBuilder.Create()
            .WithController(controller)
            .Build();
        // Assert
        Expect(controller.ControllerContext)
            .To.Be(result);
        Expect(result.ActionDescriptor.ControllerName)
            .To.Equal("My");
        Expect(result.ActionDescriptor.ControllerTypeInfo)
            .To.Equal(typeof(MyController).GetTypeInfo());
    }

    public class MyController : ControllerBase
    {
    }
}