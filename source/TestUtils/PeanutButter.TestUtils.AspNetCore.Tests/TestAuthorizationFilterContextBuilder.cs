using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Controllers;
using NUnit.Framework;
using NExpect;
using PeanutButter.TestUtils.AspNetCore.Builders;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using static NExpect.Expectations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using PeanutButter.Utils;
using static NExpect.AspNetCoreExpectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TestUtils.AspNetCore.Tests
{
    [TestFixture]
    public class TestAuthorizationFilterContextBuilder
    {
        [Test]
        public void ShouldSetHttpContext()
        {
            // Arrange
            // Act
            var result = AuthorizationFilterContextBuilder
                .ForController<MyController>()
                .Build();
            // Assert
            Expect(result.HttpContext)
                .Not.To.Be.Null();
            Expect(result.HttpContext)
                .To.Be.An.Instance.Of<FakeHttpContext>();
            Expect(result.HttpContext.Request.Headers)
                .To.Contain.Only(1)
                .Equal.To(DefaultContentTypeHeader);
        }

        [Test]
        public void ShouldSetControllerMeta()
        {
            // Arrange
            // Act
            var result = AuthorizationFilterContextBuilder
                .ForController<MyController>()
                .Build();
            // Assert
            Expect(result.ActionDescriptor)
                .To.Be.An.Instance.Of<ControllerActionDescriptor>();
            var controllerDescriptor = result.ActionDescriptor as ControllerActionDescriptor;
            Expect(controllerDescriptor!.ControllerName)
                .To.Equal("My");
            Expect(controllerDescriptor.ControllerTypeInfo)
                .To.Equal(typeof(MyController).GetTypeInfo());
        }

        [Test]
        public void ShouldSetActionMeta()
        {
            // Arrange
            // Act
            var result = AuthorizationFilterContextBuilder
                .ForController<MyController>()
                .ForAction(nameof(MyController.Moo))
                .Build();
            // Assert
            var controllerDescriptor = result.ActionDescriptor as ControllerActionDescriptor;
            Expect(controllerDescriptor!.MethodInfo)
                .To.Equal(typeof(MyController).GetMethod(nameof(MyController.Moo)));
            Expect(controllerDescriptor.ActionName)
                .To.Equal(nameof(MyController.Moo));
            Expect(controllerDescriptor.DisplayName)
                .To.Equal(nameof(MyController.Moo));
        }

        [Test]
        public void ShouldBeAbleToSetRequestJsonBodyFromString()
        {
            // Arrange
            var o = new Poco
            {
                Id = 1,
                Value = "moo"
            };
            var json = JsonSerializer.Serialize(o);
            // Act
            var result = AuthorizationFilterContextBuilder
                .ForController<MyController>()
                .ForAction(nameof(MyController.Moo))
                .WithJsonBody(json)
                .Build();
            // Assert
            var body = result.HttpContext.Request.Body.ReadAllText();
            var deserialized = JsonSerializer.Deserialize<Poco>(body);
            Expect(deserialized)
                .To.Deep.Equal(o);
        }

        [Test]
        public void ShouldBeAbleToSetRequestJsonBodyFromObject()
        {
            // Arrange
            var o = new Poco
            {
                Id = 1,
                Value = "moo"
            };
            // Act
            var result = AuthorizationFilterContextBuilder
                .ForController<MyController>()
                .ForAction(nameof(MyController.Moo))
                .WithJsonBody(o)
                .Build();
            // Assert
            var body = result.HttpContext.Request.Body.ReadAllText();
            var deserialized = JsonSerializer.Deserialize<Poco>(body);
            Expect(deserialized)
                .To.Deep.Equal(o);
        }

        [Test]
        public void ShouldBeAbleToSetRequestCookie()
        {
            // Arrange
            var key = GetRandomString();
            var value = GetRandomString();
            // Act
            var result = AuthorizationFilterContextBuilder
                .ForController<MyController>()
                .ForAction(nameof(MyController.Moo))
                .WithRequestCookie(key, value)
                .Build();
            // Assert
            Expect(result.HttpContext.Request.Cookies)
                .To.Contain.Key(key)
                .With.Value(value);
        }

        [Test]
        public void ShouldBeAbleToSetRequestCookies()
        {
            // Arrange
            var key = GetRandomString();
            var value = GetRandomString();
            var cookies = new Dictionary<string, string>()
            {
                [key] = value
            };
            // Act
            var result = AuthorizationFilterContextBuilder
                .ForController<MyController>()
                .ForAction(nameof(MyController.Moo))
                .WithRequestCookies(cookies)
                .Build();
            // Assert
            Expect(result.HttpContext.Request.Cookies)
                .To.Contain.Key(key)
                .With.Value(value);
        }

        [Test]
        public void ShouldBeAbleToSetRequestUrl()
        {
            // Arrange
            var expected = GetRandomHttpUrl();
            // Act
            var result = AuthorizationFilterContextBuilder
                .ForController<MyController>()
                .ForAction(nameof(MyController.Moo))
                .WithRequestUrl(expected)
                .Build();
            // Assert
            Expect(result.HttpContext.Request.FullUrl())
                .To.Equal(new Uri(expected));
        }


        public class Poco
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }

        public class MyController : ControllerBase
        {
            public void Moo()
            {
            }
        }

        private static readonly KeyValuePair<string, StringValues> DefaultContentTypeHeader =
            new("Content-Type", "text/plain");
    }
}