using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using NExpect;
using PeanutButter.TestUtils.AspNetCore.Builders;
using static NExpect.Expectations;

namespace PeanutButter.Utils.NetCore.Tests
{
    [TestFixture]
    public class TestDeepEqualityTester
    {
        [Test]
        public void ShouldNotStackOverflowWhenComparingEnumValues()
        {
            // Arrange
            var left = new { LogLevel = LogLevel.Critical };
            var right = new { LogLevel = LogLevel.Critical };
            var sut = Create(left, right);
            sut.RecordErrors = true;
            sut.VerbosePropertyMismatchErrors = false;
            sut.FailOnMissingProperties = true;
            sut.IncludeFields = true;
            sut.OnlyTestIntersectingProperties = false;
            // Act
            var result = sut.AreDeepEqual();
            // Assert
            Expect(result)
                .To.Be.True();
        }

        [Test]
        public void ShouldProduceCorrectMessageWhenEnabledForPropertiesOfDifferentTypes()
        {
            // Arrange
            int intVal = 1;
            uint uintVal = 1;
            var left = new { foo = intVal };
            var right = new { foo = uintVal };
            var sut = Create(left, right);
            var expected = "Source property 'foo' has type 'Int32' but comparison property has type 'UInt32'";
            sut.RecordErrors = true;

            // Act
            Expect(sut.AreDeepEqual())
                .To.Be.False();
            // Assert
            Expect(sut.Errors.Single())
                .To.Equal(expected);
        }

        [Test]
        public void ShouldNotBreakOnInfiniteRecursion()
        {
            // Arrange
            var node1 = new Node();
            var node2 = new Node();
            node1.Child = node2;
            node2.Child = node1;
            var node3 = new Node()
            {
                Child = new Node()
                {
                    Child = new Node()
                }
            };
            var sut = Create(node1, node3);
            // Act
            sut.RecordErrors = true;
            sut.VerbosePropertyMismatchErrors = false;
            sut.FailOnMissingProperties = true;
            sut.IncludeFields = true;
            sut.OnlyTestIntersectingProperties = false;
            Expect(sut.AreDeepEqual())
                .To.Be.False();
            // Assert
        }

        [Test]
        public void ShouldNotBreakOnPathString()
        {
            // Arrange
            var s1 = new PathString();
            var s2 = new PathString();
            var sut = Create(s1, s2);
            // Act
            var result = sut.AreDeepEqual();
            // Assert
            Expect(result)
                .To.Be.True();
        }

        [Test]
        public void ComparingKeyValuePairsWithComplexParts()
        {
            // Arrange
            var kvp1 = new KeyValuePair<string, string[]>("a", new[] { "b" });
            var kvp2 = new KeyValuePair<string, string[]>("a", new[] { "b" });
            var sut = Create(kvp1, kvp2);
            // Act
            var result = sut.AreDeepEqual();
            // Assert
            Expect(result)
                .To.Be.True();
        }

        [Test]
        public void ShouldNotBreakOnFakeHttpRequest()
        {
            // Arrange
            var req1 = HttpRequestBuilder.Create()
                .Randomize()
                .Build();
            var req2 = HttpRequestBuilder.Create()
                .Randomize()
                .Build();
            var sut = Create(req1, req2);
            // Act
            var result = sut.AreDeepEqual();
            // Assert
            Expect(result)
                .To.Be.False();
        }


        public class Node
        {
            public Node Child { get; set; }
            public string Name { get; set; } = "name";
        }

        public enum LogLevel
        {
            Trace,
            Debug,
            Information,
            Warning,
            Error,
            Critical,
            None,
        }

        private static DeepEqualityTester Create(object obj1, object obj2)
        {
            var sut = new DeepEqualityTester(obj1, obj2) { RecordErrors = true };
            return sut;
        }
    }
}