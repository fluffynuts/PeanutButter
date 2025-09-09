using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using NUnit.Framework;

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
        Expect(result).To.Equal(expected);
    }

    private static readonly Tuple<object, string>[] ComplexSource =
    {
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
    };

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
        Expect(result).To.Equal(data.Item2.TrimStart().Replace("\r", ""));
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
            }.Stringify()
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
        Expect(result).To.Equal(expected);
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
        Expect(localResult).To.Start.With(expectedPre)
            .And.End.With(" (Local)");
        Expect(utcResult).To.Start.With(expectedPre)
            .And.End.With(" (Utc)");
        Expect(unspecifiedResult).To.Start.With(expectedPre)
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
        Expect(result.RegexReplace("\\s", "")).To.Equal(raw);
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
        Expect(result).To.Equal("<node>1</node>");
    }

    public class Node
    {
        public Node Parent { get; set; }
        public string Name { get; set; }
    }

    [Timeout(2000)]
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
        var result = dict.Stringify();
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