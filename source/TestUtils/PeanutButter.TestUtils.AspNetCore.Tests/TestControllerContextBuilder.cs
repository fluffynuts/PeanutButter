using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Mvc;
using PeanutButter.TestUtils.AspNetCore.Builders;
using PeanutButter.Utils;

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

    [Test]
    public void ShouldBeAbleToSetARequestHeader()
    {
        // Arrange
        var key = GetRandomString();
        var value = GetRandomString();
        // Act
        var result = ControllerContextBuilder.Create()
            .WithRequestHeader(key, value)
            .Build();
        // Assert
        Expect(result.HttpContext.Request.Headers)
            .To.Contain.Key(key)
            .With.Value(value);
    }

    [Test]
    public void ShouldBeAbleToSetFullRequest()
    {
        // Arrange
        var headers = GetRandom<Dictionary<string, string>>();
        var url = GetRandomHttpsUrlWithPath();
        var req = HttpRequestBuilder.Create()
            .WithUrl(url)
            .WithHeaders(headers)
            .Build();
        
        // Act
        var sut = ControllerContextBuilder.Create()
            .WithRequest(req)
            .Build();
        
        // Assert
        Expect(sut.HttpContext.Request.Headers.ToDictionary())
            .To.Deep.Equal(headers);
        Expect(sut.HttpContext.Request.FullUrl().ToString().ToLower())
            .To.Equal(url.ToLower());
    }

    [Test]
    public void ShouldNotClobberRequestHeaders()
    {
        // Arrange
        var headers = GetRandom<Dictionary<string, string>>();
        Expect(headers)
            .Not.To.Be.Empty();
        var req = HttpRequestBuilder.Create()
            .WithMethod(HttpMethod.Get)
            .WithRandomUrl()
            .WithHeaders(headers)
            .Build();
        var ctx = ControllerContextBuilder.Create()
            .WithRequest(req)
            .Build();

        // Act
        Expect(ctx.HttpContext.Request.Headers.ToDictionary())
            .To.Equal(headers);

        // Assert
    }

    public class MyController : ControllerBase
    {
    }
}