using System;
using System.Collections.Generic;
using NExpect;
using NUnit.Framework;
using static NExpect.Expectations;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestWebObjectExtensions
    {
        [TestFixture]
        public class AsQueryStringParameters
        {
            [Test]
            public void ShouldReturnEmptyStringForNull()
            {
                // Arrange
                var o = null as object;
                // Act
                var result = o.AsQueryStringParameters();
                // Assert
                Expect(result)
                    .To.Equal("");
            }

            [Test]
            public void ShouldReturnSinglePropertyParameter()
            {
                // Arrange
                var o = new { foo = "bar" };
                var expected = "foo=bar";
                // Act
                var result = o.AsQueryStringParameters();
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldReturnMultiplePropertyParameters()
            {
                // Arrange
                var o = new { foo = "bar", bar = "quux" };
                var expected = "foo=bar&bar=quux";
                // Act
                var result = o.AsQueryStringParameters();
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldEscapeBitsNeedingEscaping()
            {
                // Arrange
                var o = new Dictionary<string, string>()
                {
                    ["foo bar"] = "foo&bar"
                };
                var expected = "foo+bar=foo%26bar";
                // Act
                var result = o.AsQueryStringParameters();
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }
        [TestFixture]
        public class AsQueryString
        {
            [Test]
            public void ShouldReturnEmptyStringForNull()
            {
                // Arrange
                var o = null as object;
                // Act
                var result = o.AsQueryString();
                // Assert
                Expect(result)
                    .To.Equal("");
            }

            [Test]
            public void ShouldHandleNullPropertyValueLikeEmptyString()
            {
                // Arrange
                var o = new { id = null as string };
                // Act
                var result = o.AsQueryString();
                // Assert
                Expect(result)
                    .To.Equal("?id=");
            }

            [Test]
            public void ShouldReturnSinglePropertyParameter()
            {
                // Arrange
                var o = new { foo = "bar" };
                var expected = "?foo=bar";
                // Act
                var result = o.AsQueryString();
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldReturnMultiplePropertyParameters()
            {
                // Arrange
                var o = new { foo = "bar", bar = "quux" };
                var expected = "?foo=bar&bar=quux";
                // Act
                var result = o.AsQueryString();
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldEscapeBitsNeedingEscaping()
            {
                // Arrange
                var o = new Dictionary<string, string>()
                {
                    ["foo bar"] = "foo&bar"
                };
                var expected = "?foo+bar=foo%26bar";
                // Act
                var result = o.AsQueryString();
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }
    }
}