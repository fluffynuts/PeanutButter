using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using HtmlAgilityPack;

namespace PeanutButter.Utils.Tests;

[TestFixture]
public class TestStringifier
{
    [TestCase("foo", "\"foo\"")]
    [TestCase(1, "1")]
    [TestCase(true, "true")]
    [TestCase(false, "false")]
    [TestCase(null, "null")]
    public void Stringify_GivenPrimitive_ShouldReturnExpected(
        object value,
        string expected
    )
    {
        //--------------- Arrange -------------------

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = value.Stringify();

        //--------------- Assert -----------------------
        Expect(result)
            .To.Equal(expected);
    }

    [Test]
    public void ShouldTruncateWhenTooManySiblingObjectsOrDepthReached()
    {
        // Arrange
        var spans = string.Concat(
            Enumerable.Repeat(
                "<span>a</span>",
                3000
            )
        );
        var document = new HtmlDocument();
        document.LoadHtml($"<div>{spans}<p>x</p></div>");
        var node = document.DocumentNode.SelectSingleNode("//p");

        // Act
        var result = node.Stringify();
        
        // Assert
        Console.WriteLine(result);
        Expect(result)
            .To.Contain("<truncated: max depth");
        Expect(result)
            .To.Contain("truncated: more than");
    }

    [TestCase(Stringifier.MAX_STRINGIFY_DEPTH)]
    public void ShouldTruncateWhenDepthPassesThreshold_(
        int maxDepth
    )
    {
        // Arrange
        var last = new Node(
            "parent",
            null
        );
        for (var i = 0; i < maxDepth; i++)
        {
            var child = new Node(
                $"child-{i}",
                last
            );
            last = child;
        }

        // Act
        var result = last.Stringify();

        // Assert
        Expect(result)
            .To.Contain("truncated");
    }

    [TestCase(Stringifier.MAX_OBJECTS)]
    public void ShouldTruncateWhenMaxObjectsPassed_(
        int maxObjects
    )
    {
        // Arrange
        var node = new Node(
            "container",
            null
        );
        for (var i = 0; i < maxObjects; i++)
        {
            var sibling = new Node(
                $"sibling-{i}",
                null
            );
            node.AddSibling(sibling);
        }
        
        // Act
        var result = node.Stringify();
        
        // Assert
        Expect(result)
            .To.Contain("truncated");
    }

    [TestCase(Stringifier.MAX_STRINGIFY_DEPTH)]
    public void ShouldNotTruncateWhenDepthWithinThreshold_(
        int maxDepth
    )
    {
        // Arrange
        var last = new Node(
            "parent",
            null
        );
        // we have to back off from maxDepth by 2 because
        // last is the root, one extra node above what's created
        // below.
        for (var i = 0; i < maxDepth - 2; i++)
        {
            var child = new Node(
                $"child-{i}",
                last
            );
            last = child;
        }

        // Act
        var result = last.Stringify();

        // Assert
        Expect(result)
            .Not.To.Contain("truncated");
    }

    private static readonly Tuple<object, string>[] ComplexSource =
    [
        Tuple.Create(
            new
            {
                foo = 1
            } as object,
            @"
{
  foo: 1
}"
        ),
        Tuple.Create(
            new
            {
                foo = new
                {
                    bar = 1
                }
            } as object,
            @"
{
  foo: {
    bar: 1
  }
}"
        )
    ];

    [TestCaseSource(nameof(ComplexSource))]
    public void Stringify_GivenObject_ShouldReturnExpected(
        Tuple<object, string> data
    )
    {
        //--------------- Arrange -------------------

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = data.Item1.Stringify();

        //--------------- Assert -----------------------
        Expect(result)
            .To.Equal(
                data.Item2.TrimStart().Replace("\r", "")
            );
    }

    [Test]
    public void CollectionsOfCollections()
    {
        // Arrange
        // Pre-Assert
        // Act
        Console.WriteLine(
            new[]
            {
                new[]
                {
                    1,
                    2
                },
                new[]
                {
                    5,
                    6,
                    7
                }
            }.StringifyCollection()
        );
        // Assert
    }

    [Test]
    public void Stringifying_PropertiesOfTypeShort()
    {
        // Arrange
        var obj = new
        {
            x = short.MaxValue
        };
        var expected = (@"{
  x: " + short.MaxValue + @"
}").Replace("\r", "");
        // Pre-Assert
        // Act
        var result = obj.Stringify();
        // Assert
        Expect(result)
            .To.Equal(expected);
    }

    [Test]
    public void ShouldInclude_Kind_ForDateTime()
    {
        // Arrange
        var src = GetRandomDate();
        var local = new DateTime(
            src.Year,
            src.Month,
            src.Day,
            src.Hour,
            src.Minute,
            src.Second,
            DateTimeKind.Local
        );
        var utc = new DateTime(
            src.Year,
            src.Month,
            src.Day,
            src.Hour,
            src.Minute,
            src.Second,
            DateTimeKind.Utc
        );
        var unspecified = new DateTime(
            src.Year,
            src.Month,
            src.Day,
            src.Hour,
            src.Minute,
            src.Second,
            DateTimeKind.Unspecified
        );
        var expectedPre = src.ToString(CultureInfo.InvariantCulture);
        // Pre-Assert
        // Act
        var localResult = local.Stringify();
        var utcResult = utc.Stringify();
        var unspecifiedResult = unspecified.Stringify();
        // Assert
        Expect(localResult)
            .To.Start.With(expectedPre)
            .And.End.With(" (Local)");
        Expect(utcResult)
            .To.Start.With(expectedPre)
            .And.End.With(" (Utc)");
        Expect(unspecifiedResult)
            .To.Start.With(expectedPre)
            .And.End.With(" (Unspecified)");
    }

    [Test]
    public void ShouldOutputXmlForXDocument()
    {
        // Arrange
        var raw = "<doc><node>1</node></doc>";
        var doc = XDocument.Parse(raw);
        // Pre-assert
        // Act
        var result = doc.Stringify();
        // Assert
        Expect(result.RegexReplace("\\s", ""))
            .To.Equal(raw);
    }

    [Test]
    public void ShouldOutputXmlForXElement()
    {
        // Arrange
        var raw = "<doc><node>1</node></doc>";
        var el = XDocument.Parse(raw).XPathSelectElement("/doc/node");
        // Pre-assert
        // Act
        var result = el.Stringify();
        // Assert
        Expect(result)
            .To.Equal("<node>1</node>");
    }

    [TestFixture]
    public class LimitingTotalObjectsRendered
    {
        [Test]
        public void ShouldStopRenderingAfterMaxTotalObjectsAndSayWhy()
        {
            // Arrange
            // 1 + 10 + 100 + 1000 nodes: no single collection, path or depth
            // limit is hit, so only a limit on the total can bound this
            var tree = Tree(
                10,
                3
            );
            // Act
            var result = tree.Stringify();
            // Assert
            Expect(result)
                .To.Contain($"<truncated: more than {Stringifier.MAX_TOTAL_OBJECTS} objects in total>");
            Expect(CountNodesRendered(result))
                .To.Be.Less.Than(Stringifier.MAX_TOTAL_OBJECTS);
        }

        [Test]
        public void ShouldNotTruncateGraphsWithinTheTotalLimit()
        {
            // Arrange
            var tree = Tree(
                3,
                2
            );
            // Act
            var result = tree.Stringify();
            // Assert
            Expect(result)
                .Not.To.Contain("truncated");
            Expect(CountNodesRendered(result))
                .To.Equal(13);
        }

        [Test]
        public void ShouldBoundOutputForHugeHtmlNode()
        {
            // Arrange
            var spans = string.Concat(
                Enumerable.Repeat(
                    "<span>a</span>",
                    3000
                )
            );
            var document = new HtmlDocument();
            document.LoadHtml($"<div>{spans}<p>x</p></div>");
            var node = document.DocumentNode.SelectSingleNode("//p");
            // Act
            var result = node.Stringify();
            // Assert
            // each rendered object has a bounded number of properties, each of
            // which may be a string of at most MAX_STRING_LENGTH
            Expect(result.Length)
                .To.Be.Less.Than(Stringifier.MAX_TOTAL_OBJECTS * 2000);
        }
    }

    [TestFixture]
    public class LimitingTimeSpent
    {
        private const string TIME_TRUNCATION = "<truncated: stringification exceeded 1s>";

        [Test]
        public void ShouldKeepWhatWasRenderedAndSayWhereItStopped_WhenRenderingObjects()
        {
            // Arrange
            var tree = Tree(
                3,
                2
            );
            // Act
            var result = Stringifier.StringifyWithClock(
                tree,
                ClockExpiringAfter(5)
            );
            // Assert
            Expect(result)
                .To.Contain("root");
            Expect(result)
                .To.Contain(TIME_TRUNCATION);
            Expect(CountNodesRendered(result))
                .To.Be.Less.Than(13);
        }

        [Test]
        public void ShouldKeepWhatWasRenderedAndSayWhereItStopped_WhenRenderingCollections()
        {
            // Arrange
            var items = Enumerable.Range(0, 10)
                .Select(i => new Node($"item-{i}", null))
                .ToList();
            // Act
            var result = Stringifier.StringifyWithClock(
                items,
                ClockExpiringAfter(20)
            );
            // Assert
            Expect(result)
                .To.Contain("item-0");
            Expect(result)
                .Not.To.Contain("item-9");
            Expect(result)
                .To.Contain(TIME_TRUNCATION);
        }

        [Test]
        public void ShouldOnlyReportTheTimeoutOnce()
        {
            // Arrange
            var tree = Tree(
                3,
                2
            );
            // Act
            var result = Stringifier.StringifyWithClock(
                tree,
                ClockExpiringAfter(12)
            );
            // Assert
            Expect(
                    System.Text.RegularExpressions.Regex.Matches(
                        result,
                        TIME_TRUNCATION
                    ).Count
                )
                .To.Equal(1);
        }

        [Test]
        public void ShouldNotTruncateWhenWithinTheTimeLimit()
        {
            // Arrange
            var tree = Tree(
                3,
                2
            );
            // Act
            var result = Stringifier.StringifyWithClock(
                tree,
                () => Stringifier.MAX_STRINGIFY_MILLISECONDS
            );
            // Assert
            Expect(result)
                .Not.To.Contain("truncated");
        }

        private static Func<long> ClockExpiringAfter(
            int calls
        )
        {
            var called = 0;
            return () => ++called <= calls
                ? 0
                : Stringifier.MAX_STRINGIFY_MILLISECONDS + 1;
        }
    }

    [TestFixture]
    public class LimitingStringLength
    {
        [Test]
        public void ShouldNotTruncateStringAtTheLimit()
        {
            // Arrange
            var input = new string(
                'a',
                Stringifier.MAX_STRING_LENGTH
            );
            // Act
            var result = input.Stringify();
            // Assert
            Expect(result)
                .To.Equal($"\"{input}\"");
        }

        [Test]
        public void ShouldTruncateStringOverTheLimitAndSayHowMuchWasShown()
        {
            // Arrange
            var input = new string(
                'a',
                Stringifier.MAX_STRING_LENGTH + 1
            );
            var shown = new string(
                'a',
                Stringifier.MAX_STRING_LENGTH
            );
            // Act
            var result = input.Stringify();
            // Assert
            Expect(result)
                .To.Equal(
                    $"\"{shown}\"<truncated: showing {Stringifier.MAX_STRING_LENGTH} of {input.Length} chars>"
                );
        }

        [Test]
        public void ShouldTruncateLongStringPropertiesOfObjects()
        {
            // Arrange
            var input = new
            {
                Text = new string(
                    'b',
                    300
                )
            };
            // Act
            var result = input.Stringify();
            // Assert
            Expect(result)
                .To.Contain($"<truncated: showing {Stringifier.MAX_STRING_LENGTH} of 300 chars>");
        }

        [Test]
        public void ShouldNotSplitASurrogatePairWhenTruncating()
        {
            // Arrange
            var input = new string(
                'a',
                Stringifier.MAX_STRING_LENGTH - 1
            ) + "\U0001F600" + "tail";
            // Act
            var result = input.Stringify();
            // Assert
            var loneHighSurrogates = result
                .Where((c, i) => char.IsHighSurrogate(c) &&
                    (i + 1 >= result.Length || !char.IsLowSurrogate(result[i + 1])))
                .ToArray();
            Expect(loneHighSurrogates)
                .To.Be.Empty();
            Expect(result)
                .To.Contain("<truncated: showing");
        }
    }

    private static Node Tree(
        int branching,
        int depth,
        string name = "root"
    )
    {
        var result = new Node(
            name,
            null
        );
        if (depth == 0)
        {
            return result;
        }

        for (var i = 0; i < branching; i++)
        {
            result.AddSibling(
                Tree(
                    branching,
                    depth - 1,
                    $"{name}.{i}"
                )
            );
        }

        return result;
    }

    private static int CountNodesRendered(
        string stringified
    )
    {
        return System.Text.RegularExpressions.Regex.Matches(
            stringified,
            "Name: "
        ).Count;
    }

    public class Node
    {
        public Node Parent { get; set; }
        public string Name { get; set; }
        public Node[] Siblings { get; private set; } = [];

        public Node()
        {
        }

        public Node(
            string name,
            Node parent
        )
        {
            Name = name;
            Parent = parent;
        }

        public void AddSibling(Node other)
        {
            Siblings = Siblings.And(other);
        }
    }

    [CancelAfter(2000)]
    [Test]
    public void ShouldNeverGetInAnInfiniteLoop()
    {
        // Arrange
        var node1 = new Node()
        {
            Name = GetRandomString(10)
        };
        var node2 = new Node()
        {
            Name = GetRandomString(10)
        };
        node1.Parent = node2;
        node2.Parent = node1;
        // Pre-assert
        // Act
        var result = node1.Stringify();
        Console.WriteLine(result);
        // Assert
        Expect(result)
            .Not.To.Be.Null.Or.Empty();
    }

    [Test]
    public void ShouldExpandArrayProperty()
    {
        // Arrange
        var key = GetRandomString(4);
        var value1 = GetRandomString(4);
        var value2 = GetRandomString(4);
        var dict = new Dictionary<string, IEnumerable<string>>()
        {
            [key] = new[]
            {
                value1,
                value2
            }.AsEnumerable()
        };
        // Act
        var result = dict.StringifyCollection();
        // Assert
        Expect(result)
            .To.Contain(key)
            .Then(value1)
            .Then(value2);
    }

    [Test]
    public void ShouldIgnoreIgnoredProperties()
    {
        // Arrange
        var data = GetRandom<SkipOneProperty>();
        // Act
        var result = data.Stringify();
        // Assert
        Expect(result)
            .Not.To.Contain("Name");
    }

    [Test]
    public void ShouldOrderPropertiesAlphabetically()
    {
        // Arrange
        var data = GetRandom<OutOfOrder>();
        // Act
        var result = data.Stringify();
        // Assert
        var lines = result.SplitIntoLines()
            .Map(s => s.Trim());
        Expect(lines)
            .To.Contain.Only(6).Items();
        Expect(lines[0])
            .To.Equal("{");
        Expect(lines[1])
            .To.Start.With("A:");
        Expect(lines[2])
            .To.Start.With("B:");
        Expect(lines[3])
            .To.Start.With("C:");
        Expect(lines[4])
            .To.Start.With("Z:");
        Expect(lines[5])
            .To.Equal("}");
    }

    public class OutOfOrder
    {
        public int Z { get; set; }
        public bool B { get; set; }
        public string A { get; set; }
        public TimeSpan C { get; set; }
    }

    public class SkipOneProperty
    {
        public int Id { get; set; }
        [SkipStringify]
        public string Name { get; set; }
    }
}