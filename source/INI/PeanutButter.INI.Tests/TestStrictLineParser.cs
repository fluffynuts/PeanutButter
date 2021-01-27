using System.Collections.Generic;
using NExpect;
using NUnit.Framework;

namespace PeanutButter.INI.Tests
{
    [TestFixture]
    public class TestStrictLineParser
    {
        public static IEnumerable<(
            string id,
            string line,
            string key,
            string value,
            string comment
            )> TestCaseGenerator()
        {
            // simplest case
            yield return ("simple-1", "key=value", "key", "value", "");
            yield return ("simple-2", "key=", "key", "", "");

            // quoted values
            yield return ("quoted-1", "key=\"value\"", "key", "value", "");
            yield return ("quoted-2", @"key=""\\server\share\path\""", "key", @"\server\share\path\", "");

            // simple line comments
            yield return ("lc-1", "key=value;comment", "key", "value", "comment");
            yield return ("lc-2", "key=value   ; comment", "key", "value", " comment");
            yield return ("lc-3", @"key=""value""; comment", "key", "value", " comment");

            // real-world case: no quote escaping, no comment
            yield return (
                "rw-1",
                @"key=""[font style=""background-color: red;""]""",
                "key",
                @"[font style=""background-color: red;""]",
                ""
            );

            // extrapolate real-world case: no quote escaping, simple inline comment
            yield return (
                "rw-2",
                @"key=""[font style=""background-color: red;""]"" ; comment ",
                "key",
                @"[font style=""background-color: red;""]",
                " comment "
            );

            // inline-comment with quote in best effort without escaping should arrive at unexpected results
            yield return (
                "unescaped-quotes-comment-quote",
                @"key=""[font style=""background-color: red;""]"" ; comment ""quoted""",
                "key",
                @"[font style=""background-color: red;""]"" ; comment ""quoted",
                ""
            );

            // inline-comment with quote in best effort with escaping should arrive at expected results
            yield return (
                "escaped-quotes-comment-quote",
                @"key=""[font style=\""background-color: red;\""]"" ; comment ""quoted""",
                "key",
                @"[font style=""background-color: red;""]",
                @" comment ""quoted"""
            );

            // value contains escaped characters
            yield return (
                "properly-escaped",
                @"key=""some \""quoted\"" value"" ; and comment",
                "key",
                @"some ""quoted"" value",
                " and comment"
            );
        }

        [TestCaseSource(nameof(TestCaseGenerator))]
        public void ShouldParse_(
            (string id, string line, string key, string value, string comment) testCase
        )
        {
            // Arrange
            var (id, line, key, value, comment) = testCase;
            var sut = Create();
            // Act
            var result = sut.Parse(line);
            // Assert
            Expectations.Expect(result.Key)
                .To.Equal(key);
            Expectations.Expect(result.Value)
                .To.Equal(value);
            Expectations.Expect(result.Comment)
                .To.Equal(comment);
        }

        private ILineParser Create()
        {
            return new StrictLineParser();
        }
    }
}