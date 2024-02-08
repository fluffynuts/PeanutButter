using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using PeanutButter.TestUtils.AspNetCore.Builders;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.AspNetCore.Tests;

[TestFixture]
public class TestFakeResponseCookies
{
    [Test]
    [Explicit("Only works if NSubstitute is used before-hand - have to figure out the secret sauce")]
    public void ShouldCreateSubstituteWherePossible()
    {
        // Arrange
        var httpResponse = HttpResponseBuilder.BuildDefault();
        var sut = FakeResponseCookies.CreateSubstitutedIfPossible(httpResponse);
        var key = GetRandomString();
        var value = GetRandomString();
        // Act
        sut.Append(key, value);
        // Assert
        // FIXME: the result _is_ a sub, but this assertion fails within NSubstitute
        // unless NSubstitute has been invoked earlier, eg via
        // var foo = SubstitutionContext.Current;
        // or
        // var foo = Substitute.For<Something>();
        Expect(sut)
            .To.Have.Received(1)
            .Append(key, value);
    }

    [Test]
    public void ShouldStoreCookieWithDefaultSettings()
    {
        // Arrange
        var sut = Create();
        var key = GetRandomString();
        var value = GetRandomString();
        // Act
        sut.Append(key, value);
        // Assert
        Expect(sut.Snapshot)
            .To.Contain.Key(key)
            .With.Value.Matched.By(
                o => o.Value == value &&
                    o.Options.DeepEquals(new CookieOptions())
            );
    }

    [Test]
    public void ShouldStoreWithProvidedOptions()
    {
        // Arrange
        var sut = Create();
        var key = GetRandomString();
        var value = GetRandomString();
        var options = GetRandom<CookieOptions>();
        // Act
        sut.Append(key, value, options);
        // Assert
        Expect(sut.Snapshot)
            .To.Contain.Key(key)
            .With.Value.Matched.By(
                o => o.Value == value &&
                    o.Options.DeepEquals(options)
            );
    }

    [Test]
    public void ShouldDeleteByKey()
    {
        // Arrange
        var sut = Create();
        var key = GetRandomString();
        var value = GetRandomString();
        sut.Append(key, value);

        // Act
        sut.Delete(key);

        // Assert
        Expect(sut.Snapshot)
            .Not.To.Contain.Key(key);
    }

    [Test]
    public void ShouldDeleteByKeyAndOptionsWhenDomainAndPathMatch()
    {
        // Arrange
        var sut = Create();
        var key = GetRandomString();
        var value = GetRandomString();
        var options = GetRandom<CookieOptions>();
        Expect(options.Domain)
            .Not.To.Be.Null.Or.Empty();
        Expect(options.Path)
            .Not.To.Be.Null.Or.Empty();
        sut.Append(key, value, options);
        var otherOptions = GetRandom<CookieOptions>();

        // Act
        sut.Delete(key, otherOptions);
        Expect(sut.Snapshot)
            .To.Contain.Key(key);
        sut.Delete(key, options);
        // Assert
        Expect(sut.Snapshot)
            .Not.To.Contain.Key(key);
    }

    [TestFixture]
    public class QuickerStoreAccess
    {
        [Test]
        public void ShouldBeAbleToIndexIntoStore()
        {
            // Arrange
            var sut = Create();
            var key = GetRandomString();
            var cookie = GetRandom<FakeCookie>();
            // Act
            sut[key] = cookie;
            // Assert
            Expect(sut.Snapshot)
                .To.Contain.Key(key)
                .With.Value.Deep.Equal.To(cookie);
        }

        [Test]
        public void ShouldBeAbleToQueryAllKeys()
        {
            // Arrange
            var sut = Create();
            var keys = GetRandomArray<string>(2, 4);
            foreach (var k in keys)
            {
                sut[k] = GetRandom<FakeCookie>();
            }

            // Act
            var result = sut.Keys;
            // Assert
            Expect(result)
                .To.Be.Equivalent.To(keys);
        }

        [Test]
        public void ShouldBeAbleToQueryAllValues()
        {
            // Arrange
            var sut = Create();
            var keys = GetRandomArray<string>(2, 4);
            var cookies = new List<FakeCookie>();
            foreach (var k in keys)
            {
                sut[k] = GetRandom<FakeCookie>();
                cookies.Add(sut[k]);
            }

            // Act
            var result = sut.Values;
            // Assert
            Expect(result)
                .To.Be.Deep.Equivalent.To(cookies);
        }

        [Test]
        public void ShouldBeAbleToQueryIfHasKey()
        {
            // Arrange
            var included = GetRandomString();
            var exluded = GetAnother(included);
            var sut = Create();
            sut[included] = GetRandom<FakeCookie>();
            // Act
            var includedResult = sut.ContainsKey(included);
            var excludedResult = sut.ContainsKey(exluded);
            // Assert

            Expect(includedResult)
                .To.Be.True();
            Expect(excludedResult)
                .To.Be.False();
        }
    }

    [TestFixture]
    public class BackingInAssociatedResponse
    {
        [Test]
        public void ShouldSetTheSetCookieHeader()
        {
            // Arrange
            var key = GetRandomString();
            var value = GetRandomString();
            var expected = $"{key}={value}; Path=/";
            var response = HttpResponseBuilder.Create()
                .WithCookies(null as IResponseCookies)
                .Build();
            var sut = Create(response);
            Expect(response.Cookies)
                .To.Be(sut);
            Expect(response.Headers["set-cookie"].ToArray())
                .To.Be.Empty();
            // Act
            sut[key] = new FakeCookie(key, value, new CookieOptions());
            // Assert
            var setCookie = response.Headers["set-cookie"].ToArray();
            Expect(setCookie)
                .To.Contain.Only(1)
                .Equal.To(expected);
        }

        [Test]
        public void ShouldSetTheSetSecureHttpOnlyCookieHeaderWithMaxAgeAndDomain()
        {
            // Arrange
            var key = GetRandomString();
            var value = GetRandomString();
            var domain = GetRandomHostname();
            var path = "/{GetRandomString()";
            var maxAge = GetRandomInt(100, 200);
            var expected = $"{key}={value}; Domain={domain}; Path={path}; Max-Age={maxAge}; Secure; HttpOnly";
            var response = HttpResponseBuilder.Create()
                .WithCookies(null as IResponseCookies)
                .Build();
            var sut = Create(response);
            Expect(response.Cookies)
                .To.Be(sut);
            Expect(response.Headers["set-cookie"].ToArray())
                .To.Be.Empty();
            // Act
            sut[key] = new FakeCookie(key, value, new CookieOptions()
            {
                Secure = true,
                HttpOnly = true,
                Domain = domain,
                Path = path,
                MaxAge = TimeSpan.FromSeconds(maxAge)
            });
            // Assert
            var setCookie = response.Headers["set-cookie"].ToArray();
            Expect(setCookie)
                .To.Contain.Only(1)
                .Equal.To(expected);
        }

        [Test]
        public void ShouldUpdateIndexerValueWhenHeaderChanges()
        {
            // Arrange
            var response = HttpResponseBuilder.Create()
                .WithCookies(null as IResponseCookies)
                .Build();
            var sut = Create(response);
            Expect(response.Cookies)
                .To.Be(sut);
            Expect(response.Headers["set-cookie"].ToArray())
                .To.Be.Empty();

            var key1 = GetRandomString();
            var value1 = GetRandomString();
            var domain = GetRandomHostname();
            var path = $"/{GetRandomString()}";
            var maxAge = GetRandomInt(100, 200);
            var key2 = GetRandomString(10);
            var value2 = GetRandomString(10);
            var header1 = $"{key1}={value1}; Domain={domain}; Path={path}; Max-Age={maxAge}; Secure; HttpOnly; SameSite=None";
            var header2 = $"{key2}={value2}";
            var expected1 = new FakeCookie(key1, value1, new CookieOptions()
            {
                Path = path,
                Domain = domain,
                MaxAge = TimeSpan.FromSeconds(maxAge),
                Secure = true,
                SameSite = SameSiteMode.None,
                HttpOnly = true,
            });
            var expected2 = new FakeCookie(key2, value2, new CookieOptions());
            // Act
            response.Headers["set-cookie"] = new StringValues(
                new[]
                {
                    header1,
                    header2
                }
            );
            var result1 = sut[key1];
            var result2 = sut[key2];

            // Assert
            Expect(result1)
                .To.Deep.Equal(expected1);
            Expect(result2)
                .To.Deep.Equal(expected2);
        }
    }

    private static FakeResponseCookies Create(
        HttpResponse associatedResponse = null
    )
    {
        return new FakeResponseCookies(associatedResponse);
    }
}