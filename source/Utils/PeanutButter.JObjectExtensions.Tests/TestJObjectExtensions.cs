using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable PossibleNullReferenceException

namespace PeanutButter.JObjectExtensions.Tests
{
    [TestFixture]
    public class TestJObjectExtensions
    {
        [Test]
        public void ToDictionary_ShouldConvertOneLevel()
        {
            // Arrange
            var json = @"
{
    int: 1,
    string: ""moo"",
    bool: true,
}
";
            var jobj = JsonConvert.DeserializeObject(json) as JObject;
            // Pre-Assert

            // Act
            var result = jobj.ToDictionary();

            // Assert

            Expect(result)
                .Not.To.Be.Null();
            Expect(result["int"])
                .To.Equal(1);
            Expect(result["string"])
                .To.Equal("moo");
            Expect(result["bool"])
                .To.Equal(true);
        }

        [Test]
        public void ToDictionary_ShouldConvertTwoLevels()
        {
            // Arrange
            var json = @"
{
    ""int"": 1,
    ""string"": ""moo"",
    ""bool"": true,
    ""sub"": {
        ""uri"": ""http://www.google.com"",
        ""nullValue"": null
    }
}
";
            var jobj = JsonConvert.DeserializeObject(json) as JObject;
            // Pre-Assert

            // Act
            var result = jobj.ToDictionary();

            // Assert

            Expect(result)
                .Not.To.Be.Null();
            Expect(result["int"])
                .To.Equal(1);
            Expect(result["string"])
                .To.Equal("moo");
            Expect(result["bool"])
                .To.Equal(true);
            Expect(result["sub"])
                .Not.To.Be.Null();
            var sub = result["sub"] as Dictionary<string, object>;
            Expect(sub["uri"])
                .To.Equal("http://www.google.com");
            Expect(sub["nullValue"])
                .To.Be.Null();
            
        }

        [Test]
        public void ToDictionary_ShouldConvertArraysOfUniformType()
        {
            // Arrange
            var json = @"
{
    ""int"": 1,
    ""string"": ""moo"",
    ""bool"": true,
    ""sub"": {
        ""uri"": ""http://www.google.com"",
        ""nullValue"": null
    },
    ""collection"": [
        ""cow"",
        ""beef"",
        ""bovine""
    ]
}
";
            var jobj = JsonConvert.DeserializeObject(json) as JObject;
            // Pre-Assert

            // Act
            var result = jobj.ToDictionary();

            // Assert

            var collection = result["collection"] as string[];
            Expect(collection)
                .Not.To.Be.Null();
            Expect(collection)
                .To.Equal(new[] { "cow", "beef", "bovine" });
        }

        [Test]
        public void ToDictionary_ShouldConvertArraysOfNonUniformType()
        {
            // Arrange
            var json = @"
{
    ""int"": 1,
    ""string"": ""moo"",
    ""bool"": true,
    ""sub"": {
        ""uri"": ""http://www.google.com"",
        ""nullValue"": null
    },
    ""collection"": [
        ""cow"",
        43,
        true
    ]
}
";
            var jobj = JsonConvert.DeserializeObject(json) as JObject;
            // Pre-Assert

            // Act
            var result = jobj.ToDictionary();

            // Assert

            var collection = result["collection"] as object[];
            Expect(collection)
                .Not.To.Be.Null();
            Expect(collection)
                .To.Equal(new object[] { "cow", 43, true } );
        }
    }
}
