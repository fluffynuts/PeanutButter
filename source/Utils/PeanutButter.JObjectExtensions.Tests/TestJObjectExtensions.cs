using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
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

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result["int"]);
            Assert.AreEqual("moo", result["string"]);
            Assert.AreEqual(true, result["bool"]);
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

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result["int"]);
            Assert.AreEqual("moo", result["string"]);
            Assert.AreEqual(true, result["bool"]);
            Assert.IsNotNull(result["sub"]);
            var sub = result["sub"] as Dictionary<string, object>;
            Assert.AreEqual("http://www.google.com", sub["uri"]);
            Assert.IsNull(sub["nullValue"]);
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
            Assert.IsNotNull(collection);
            CollectionAssert.AreEqual(
                new[] { "cow", "beef", "bovine" },
                collection
            );
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
            Assert.IsNotNull(collection);
            CollectionAssert.AreEqual(
                new object[] { "cow", 43, true },
                collection
            );
        }
    }
}
