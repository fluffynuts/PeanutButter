using System.Collections.Generic;
using NUnit.Framework;
using NExpect;
using static NExpect.Expectations;

namespace PeanutButter.INI.Tests
{
    [TestFixture]
    public class TestBestEffortLineParser
    {
        public static IEnumerable<(
            string id,
            string line,
            string key,
            string value,
            string comment
            )> TestCaseGenerator()
        {
            // pure comment line
            yield return ("comment", "; this is a comment", "", null, " this is a comment");
            // section head
            yield return ("section", "[section]", "[section]", null, "");
            yield return ("commented-section", "[section] ; some comment", "[section]", null, " some comment");
            // simplest case
            yield return ("simple-1", "key=value", "key", "value", "");
            yield return ("simple-2", "key=", "key", "", "");
            yield return ("simple-3", "key = value", "key", "value", "");
            yield return ("simple-4", @"key = ""value""", "key", "value", "");

            // quoted values
            yield return ("quoted-1", "key=\"value\"", "key", "value", "");
            yield return ("quoted-2", @"key=""\\server\share\path\""", "key", @"\\server\share\path\", "");

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
            var (_, line, key, value, comment) = testCase;
            var sut = Create();
            // Act
            var result = sut.Parse(line);
            // Assert
            Expect(result.Key)
                .To.Equal(key);
            Expect(result.Value)
                .To.Equal(value);
            Expect(result.Comment)
                .To.Equal(comment);
        }

        [Test]
        public void ReportedIssueWithUnescapedQuotes()
        {
            // Arrange
            var content = @"
Email_Message=""<HTML> <HEAD> <title>message</title> <meta http-equiv=""content-type"" content=""text/html; charset=ISO-8859-1""> <style type=""text/css"">  body{font-family: Verdana, Geneva, sans-serif; font-size:10pt;}  td{font-family: Verdana, Geneva, sans-serif;font-size:10pt;}  p{margin-top: 10px; margin-bottom: 10px;}  ol,ul{margin-top: -10px; margin-bottom: -10px;} </style> </HEAD> <BODY><h4>Folder_Not_Empty Evant</h4><p>File: <strong>{FileName}</strong>&nbsp;<br>Extension: <strong>{Extension}</strong></p><p>Was moved <br>From: <strong>{Watch_Folder}</strong><br>To: <strong>{Move2_Folder}</strong></p><p>Date Timestamp: <strong>{Date Timestamp}</strong></p><p>Moved File Full Path: <strong>{MovedFileFullPath}</strong><br><br><br></p><p><font style=""background-color: rgb(255, 255, 192);""><br></font></p><p></p></BODY></HTML>""
".Trim();
            var expectedKey = "Email_Message";
            var expectedValue = @"<HTML> <HEAD> <title>message</title> <meta http-equiv=""content-type"" content=""text/html; charset=ISO-8859-1""> <style type=""text/css"">  body{font-family: Verdana, Geneva, sans-serif; font-size:10pt;}  td{font-family: Verdana, Geneva, sans-serif;font-size:10pt;}  p{margin-top: 10px; margin-bottom: 10px;}  ol,ul{margin-top: -10px; margin-bottom: -10px;} </style> </HEAD> <BODY><h4>Folder_Not_Empty Evant</h4><p>File: <strong>{FileName}</strong>&nbsp;<br>Extension: <strong>{Extension}</strong></p><p>Was moved <br>From: <strong>{Watch_Folder}</strong><br>To: <strong>{Move2_Folder}</strong></p><p>Date Timestamp: <strong>{Date Timestamp}</strong></p><p>Moved File Full Path: <strong>{MovedFileFullPath}</strong><br><br><br></p><p><font style=""background-color: rgb(255, 255, 192);""><br></font></p><p></p></BODY></HTML>";
            var sut = Create();
            // Act
            var result = sut.Parse(content);
            // Assert
            Expect(result.Key)
                .To.Equal(expectedKey);
            Expect(result.Value)
                .To.Equal(expectedValue);
            Expect(result.Comment)
                .To.Be.Empty();
            Expect(result.ContainedEscapedEntities)
                .To.Be.False();
        }

        private ILineParser Create()
        {
            return new BestEffortLineParser();
        }
    }
}