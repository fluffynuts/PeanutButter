using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using PeanutButter.DuckTyping.Extensions;
using PeanutButter.TestUtils.AspNetCore.Builders;

namespace PeanutButter.TestUtils.AspNetCore.Tests;

[TestFixture]
public class TestHttpResponseExtensions
{
    [TestFixture]
    public class ParseCookies
    {
        [TestFixture]
        public class ActingOnAspNetCoreHttpResponse
        {
            [Test]
            public void ShouldParseKeyAndValueAndSetPathRoot()
            {
                // Arrange
                var key = GetRandomString();
                var value = GetRandomString();
                var res = HttpResponseBuilder.Create()
                    .WithCookie(key, value)
                    .Build();
                var expected = new Cookie(
                    key,
                    value
                )
                {
                    Path = "/"
                }.DuckAs<ICookie>();
                // Act
                var result = res.ParseCookies().ToArray();
                // Assert
                Expect(result)
                    .To.Contain.Only(1).Item();
                Expect(result[0].DuckAs<ICookie>())
                    .To.Deep.Equal(expected);
            }

            [Test]
            public void ShouldParseMultipleCookiesProperly()
            {
                // Arrange
                var k1 = GetRandomString();
                var k2 = GetAnother(k1);
                var v1 = GetRandomString();
                var v2 = GetRandomString();
                var res = HttpResponseBuilder.Create()
                    .WithCookie(k1, v1)
                    .WithCookie(k2, v2)
                    .Build();

                // Act
                var result = res.ParseCookies().ToArray();
                // Assert
                Expect(result)
                    .To.Contain.Only(2).Items();
                Expect(result)
                    .To.Contain.Exactly(1)
                    .Matched.By(
                        cookie => cookie.Name == k1 && cookie.Value == v1
                    );
                Expect(result)
                    .To.Contain.Exactly(1)
                    .Matched.By(
                        cookie => cookie.Name == k2 && cookie.Value == v2
                    );
            }

            [Test]
            public void ShouldParseSecureCookie()
            {
                // Arrange
                var key = GetRandomString();
                var value = GetRandomString();
                var res = HttpResponseBuilder.Create()
                    .WithCookie(
                        key,
                        value,
                        new CookieOptions()
                        {
                            Secure = true
                        }
                    )
                    .Build();
                var expected = new Cookie(
                    key,
                    value
                )
                {
                    Secure = true,
                    Path = "/"
                }.DuckAs<ICookie>();
                // Act
                var result = res.ParseCookies().ToArray();
                // Assert
                Expect(result)
                    .To.Contain.Only(1).Item();
                Expect(result[0].DuckAs<ICookie>())
                    .To.Deep.Equal(expected);
            }

            [Test]
            public void ShouldParseHttpOnlyCookie()
            {
                // Arrange
                var key = GetRandomString();
                var value = GetRandomString();
                var res = HttpResponseBuilder.Create()
                    .WithCookie(
                        key,
                        value,
                        new CookieOptions()
                        {
                            HttpOnly = true
                        }
                    )
                    .Build();
                var expected = new Cookie(
                    key,
                    value
                )
                {
                    HttpOnly = true,
                    Path = "/"
                }.DuckAs<ICookie>();
                // Act
                var result = res.ParseCookies().ToArray();
                // Assert
                Expect(result)
                    .To.Contain.Only(1).Item();
                Expect(result[0].DuckAs<ICookie>())
                    .To.Deep.Equal(expected);
            }

            [Test]
            public void ShouldParseTheDomain()
            {
                // Arrange
                var key = GetRandomString();
                var value = GetRandomString();
                var res = HttpResponseBuilder.Create()
                    .WithCookie(
                        key,
                        value,
                        new CookieOptions()
                        {
                            Domain = "foo.bar.com"
                        }
                    )
                    .Build();
                var expected = new Cookie(
                    key,
                    value
                )
                {
                    Domain = "foo.bar.com",
                    Path = "/"
                }.DuckAs<ICookie>();
                // Act
                var result = res.ParseCookies().ToArray();
                // Assert
                Expect(result)
                    .To.Contain.Only(1).Item();
                Expect(result[0].DuckAs<ICookie>())
                    .To.Deep.Equal(expected);
            }

            [Test]
            public void ShouldParseExpires()
            {
                // Arrange
                var expires = GetRandomDateTimeOffset(
                    DateTimeOffset.Now
                );
                var key = GetRandomString();
                var value = GetRandomString();
                var res = HttpResponseBuilder.Create()
                    .WithCookie(
                        key,
                        value,
                        new CookieOptions()
                        {
                            Expires = expires
                        }
                    )
                    .Build();
                // Act
                var result = res.ParseCookies().ToArray();
                // Assert
                Expect(result)
                    .To.Contain.Only(1).Item();
                Expect(result[0].Expires)
                    .To.Approximately.Equal(expires.DateTime);
            }
        }

        [TestFixture]
        public class ActingOnHttpResponseHeaders
        {
            [Test]
            public void ShouldParseKeyAndValueAndSetPathRoot()
            {
                // Arrange
                var key = GetRandomString();
                var value = GetRandomString();
                var message = new HttpResponseMessage();
                message.Headers.Add(
                    "Set-Cookie",
                    $"{key}={value};path=/"
                );

                var expected = new Cookie(
                    key,
                    value
                )
                {
                    Path = "/"
                }.DuckAs<ICookie>();
                // Act
                var result = message.ParseCookies().ToArray();
                // Assert
                Expect(result)
                    .To.Contain.Only(1).Item();
                Expect(result[0].DuckAs<ICookie>())
                    .To.Deep.Equal(expected);
            }

            [Test]
            public void ShouldParseMultipleCookiesProperly()
            {
                // Arrange
                var k1 = GetRandomString();
                var k2 = GetAnother(k1);
                var v1 = GetRandomString();
                var v2 = GetRandomString();
                var res = new HttpResponseMessage();
                res.Headers.Add(
                    "Set-Cookie",
                    $"{k1}={v1};path=/"
                );
                res.Headers.Add(
                    "Set-Cookie",
                    $"{k2}={v2};path=/"
                );

                // Act
                var result = res.ParseCookies().ToArray();
                // Assert
                Expect(result)
                    .To.Contain.Only(2).Items();
                Expect(result)
                    .To.Contain.Exactly(1)
                    .Matched.By(
                        cookie => cookie.Name == k1 && cookie.Value == v1
                    );
                Expect(result)
                    .To.Contain.Exactly(1)
                    .Matched.By(
                        cookie => cookie.Name == k2 && cookie.Value == v2
                    );
            }

            [Test]
            public void ShouldParseSecureCookie()
            {
                // Arrange
                var key = GetRandomString();
                var value = GetRandomString();
                var res = new HttpResponseMessage();
                res.Headers.Add(
                    "set-cookie",
                    $"{key}={value};path=/;secure"
                );
                var expected = new Cookie(
                    key,
                    value
                )
                {
                    Secure = true,
                    Path = "/"
                }.DuckAs<ICookie>();
                // Act
                var result = res.ParseCookies().ToArray();
                // Assert
                Expect(result)
                    .To.Contain.Only(1).Item();
                Expect(result[0].DuckAs<ICookie>())
                    .To.Deep.Equal(expected);
            }

            [Test]
            public void ShouldParseHttpOnlyCookie()
            {
                // Arrange
                var key = GetRandomString();
                var value = GetRandomString();
                var res = new HttpResponseMessage();
                res.Headers.Add(
                    "set-Cookie",
                    $"{key}={value};path=/;httponly"
                );
                var expected = new Cookie(
                    key,
                    value
                )
                {
                    HttpOnly = true,
                    Path = "/"
                }.DuckAs<ICookie>();
                // Act
                var result = res.ParseCookies().ToArray();
                // Assert
                Expect(result)
                    .To.Contain.Only(1).Item();
                Expect(result[0].DuckAs<ICookie>())
                    .To.Deep.Equal(expected);
            }

            [Test]
            public void ShouldParseTheDomain()
            {
                // Arrange
                var key = GetRandomString();
                var value = GetRandomString();
                var res = new HttpResponseMessage();
                res.Headers.Add(
                    "Set-cookie",
                    $"{key}={value};path=/;domain=foo.bar.com"
                );
                var expected = new Cookie(
                    key,
                    value
                )
                {
                    Domain = "foo.bar.com",
                    Path = "/"
                }.DuckAs<ICookie>();
                // Act
                var result = res.ParseCookies().ToArray();
                // Assert
                Expect(result)
                    .To.Contain.Only(1).Item();
                Expect(result[0].DuckAs<ICookie>())
                    .To.Deep.Equal(expected);
            }

            [Test]
            public void ShouldParseExpires()
            {
                // Arrange
                var expires = GetRandomDateTimeOffset(
                    DateTimeOffset.Now
                );
                var key = GetRandomString();
                var value = GetRandomString();
                var res = new HttpResponseMessage();
                res.Headers.Add(
                    "set-cookie",
                    $"{WebUtility.UrlEncode(key)}={WebUtility.UrlEncode(value)};expires={expires:r}"
                );
                // Act
                var result = res.ParseCookies().ToArray();
                // Assert
                Expect(result)
                    .To.Contain.Only(1).Item();
                Expect(result[0].Expires)
                    .To.Approximately.Equal(expires.DateTime);
            }
        }
    }
}

public interface ICookie
{
    string Name { get; set; }
    string Value { get; set; }
    string Path { get; set; }
    bool HttpOnly { get; set; }
    bool Secure { get; set; }
    string Domain { get; set; }
    DateTime Expires { get; set; }
}