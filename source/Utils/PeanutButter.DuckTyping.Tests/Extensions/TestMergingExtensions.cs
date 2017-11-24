using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using NExpect;
using static NExpect.Expectations;
using NUnit.Framework;
using PeanutButter.DuckTyping.Exceptions;
using PeanutButter.DuckTyping.Extensions;
using static PeanutButter.RandomGenerators.RandomValueGen;

// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.DuckTyping.Tests.Extensions
{
    [TestFixture]
    public class TestMergingExtensions
    {
        [TestFixture]
        public class VerifyingMerge
        {
            [TestFixture]
            public class UnFuzzy
            {
                [Test]
                public void OperatingOnCollectionofOneWhichCanBackTheInterface_ShouldReturnTrue()
                {
                    // Arrange
                    var src1 = new Dictionary<string, object>()
                    {
                        ["Id"] = 1,
                        ["Name"] = "Bob"
                    };

                    // Act
                    var result = new[] {src1}.CanMergeAs<IMerge1>();
                    // Assert
                    Expect(result).To.Be.True();
                }

                [Test]
                public void OperatingOnCollectionofTwoWhichCanBackTheInterface_ShouldReturnTrue()
                {
                    // Arrange
                    var src1 = new Dictionary<string, object>()
                    {
                        ["Id"] = 1
                    };
                    var src2 = new Dictionary<string, object>()
                    {
                        ["Name"] = "Bob"
                    };

                    // Act
                    var result = new[] {src1, src2}.CanMergeAs<IMerge1>();
                    // Assert
                    Expect(result).To.Be.True();
                }

                [Test]
                public void OperatingOnCollectionofTwoWithMoreSpecificTypesWhichCanBackTheInterface_ShouldReturnTrue()
                {
                    // Arrange
                    var src1 = new Dictionary<string, int>()
                    {
                        ["Id"] = 1
                    };
                    var src2 = new Dictionary<string, string>()
                    {
                        ["Name"] = "Bob"
                    };

                    // Act
                    var result = new object[] {src1, src2}.CanMergeAs<IMerge1>();
                    // Assert
                    Expect(result).To.Be.True();
                }

                [Test]
                public void OperatingOnMultiplesIncludingNameValueCollection_WhenShouldPass_Does()
                {
                    // Arrange
                    var src1 = new Dictionary<string, int>()
                    {
                        ["Id"] = 1
                    };
                    var src2 = new NameValueCollection()
                    {
                        {"Name", "Bob"}
                    };

                    // Pre-Assert

                    // Act
                    var result = new object[] {src1, src2}.CanMergeAs<IMerge1>();

                    // Assert
                    Expect(result).To.Be.True();
                }

                [Test]
                public void OperatingOnMultiplesIncludingObject_WhenShouldPass_Does()
                {
                    // Arrange
                    var src1 = new {Id = 1};
                    var src2 = new NameValueCollection()
                    {
                        {"Name", "Bob"}
                    };

                    // Pre-Assert

                    // Act
                    var result = new object[] {src1, src2}.CanMergeAs<IMerge1>();

                    // Assert
                    Expect(result).To.Be.True();
                }

                [Test]
                public void OperatingOnMultiplesIncludingConnectionStringsCollection_WhenShouldPass_Does()
                {
                    // Arrange
                    var src1 = new {Id = 1};
                    var src2 = new ConnectionStringSettingsCollection
                    {
                        new ConnectionStringSettings(
                            "Name", "Bob"
                        )
                    };

                    // Pre-Assert

                    // Act
                    var result = new object[] {src1, src2}.CanMergeAs<IMerge1>();

                    // Assert
                    Expect(result).To.Be.True();
                }
            }

            [TestFixture]
            public class Fuzzy
            {
                [Test]
                public void OperatingOnCollectionofOneWhichCanBackTheInterface_ShouldReturnTrue()
                {
                    // Arrange
                    var src1 = new Dictionary<string, object>()
                    {
                        ["Id"] = 1,
                        ["Name"] = "Bob"
                    };

                    // Act
                    var result = new[] {src1}.CanFuzzyMergeAs<IMerge1>();
                    // Assert
                    Expect(result).To.Be.True();
                }

                [Test]
                public void OperatingOnCollectionofTwoWhichCanBackTheInterface_ShouldReturnTrue()
                {
                    // Arrange
                    var src1 = new Dictionary<string, object>()
                    {
                        ["Id"] = 1
                    };
                    var src2 = new Dictionary<string, object>()
                    {
                        ["Name"] = "Bob"
                    };

                    // Act
                    var result = new[] {src1, src2}.CanFuzzyMergeAs<IMerge1>();
                    // Assert
                    Expect(result).To.Be.True();
                }

                [Test]
                public void OperatingOnCollectionofTwoWithMoreSpecificTypesWhichCanBackTheInterface_ShouldReturnTrue()
                {
                    // Arrange
                    var src1 = new Dictionary<string, int>()
                    {
                        ["Id"] = 1
                    };
                    var src2 = new Dictionary<string, string>()
                    {
                        ["Name"] = "Bob"
                    };

                    // Act
                    var result = new object[] {src1, src2}.CanFuzzyMergeAs<IMerge1>();
                    // Assert
                    Expect(result).To.Be.True();
                }

                [Test]
                public void OperatingOnMultiplesIncludingNameValueCollection_WhenShouldPass_Does()
                {
                    // Arrange
                    var src1 = new Dictionary<string, int>()
                    {
                        ["Id"] = 1
                    };
                    var src2 = new NameValueCollection()
                    {
                        {"Name", "Bob"}
                    };

                    // Pre-Assert

                    // Act
                    var result = new object[] {src1, src2}.CanFuzzyMergeAs<IMerge1>();

                    // Assert
                    Expect(result).To.Be.True();
                }

                [Test]
                public void OperatingOnMultiplesIncludingObject_WhenShouldPass_Does()
                {
                    // Arrange
                    var src1 = new {Id = 1};
                    var src2 = new NameValueCollection()
                    {
                        {"Name", "Bob"}
                    };

                    // Pre-Assert

                    // Act
                    var result = new object[] {src1, src2}.CanFuzzyMergeAs<IMerge1>();

                    // Assert
                    Expect(result).To.Be.True();
                }
            }
        }

        [TestFixture]
        public class Merging
        {
            [TestFixture]
            public class UnFuzzy
            {
                [TestFixture]
                public class OperatingOnCollectionOfOne
                {
                    [Test]
                    public void WhenCanBackTheInterface_ShouldReturnDuck()
                    {
                        // Arrange
                        var src1 = new Dictionary<string, object>()
                        {
                            ["Id"] = 1,
                            ["Name"] = "Bob"
                        };

                        // Act
                        var result = new[] {src1}.MergeAs<IMerge1>();
                        // Assert
                        Expect(result).Not.To.Be.Null();
                        Expect(result.Id).To.Equal(1);
                        Expect(result.Name).To.Equal("Bob");
                    }

                    [Test]
                    public void WhenCannotBackTheInterface_ShouldReturnNull()
                    {
                        // Arrange
                        var src1 = new Dictionary<string, object>()
                        {
                            ["Identifier"] = 1,
                            ["Name"] = "Bob"
                        };

                        // Act
                        var result = new[] {src1}.MergeAs<IMerge1>();
                        // Assert
                        Expect(result).To.Be.Null();
                    }

                    [Test]
                    public void WhenCannotBackTheInterfaceAndInstructedToThrow_ShouldThrow()
                    {
                        // Arrange
                        var src1 = new Dictionary<string, object>()
                        {
                            ["Identifier"] = 1,
                            ["Name"] = "Bob"
                        };

                        // Act
                        Expect(() => new[] {src1}.MergeAs<IMerge1>(true))
                            .To.Throw<UnDuckableException>();
                        // Assert
                    }
                }

                [TestFixture]
                public class OperatingOnCollectionOfMoreThanOneWhichTogetherCanBackTheInterface
                {
                    [Test]
                    public void TwoObjects()
                    {
                        // Arrange
                        var first = new
                        {
                            Id = GetRandomInt()
                        };
                        var second = new
                        {
                            Name = GetRandomString()
                        };
                        // Pre-Assert
                        // Act
                        var result = new object[] {first, second}.MergeAs<IMerge1>();
                        // Assert
                        Expect(result).Not.To.Be.Null();
                        Expect(result.Id).To.Equal(first.Id);
                        Expect(result.Name).To.Equal(second.Name);
                    }

                    [Test]
                    public void DictionaryAndNameValueCollection()
                    {
                        // Arrange
                        var expectedId = GetRandomInt();
                        var expectedName = GetRandomString();
                        var first = new Dictionary<string, int>()
                        {
                            ["Id"] = expectedId
                        };
                        var second = new NameValueCollection()
                        {
                            {"Name", expectedName}
                        };
                        // Pre-Assert
                        // Act
                        var result = new object[] {first, second}.MergeAs<IMerge1>();
                        // Assert
                        Expect(result).Not.To.Be.Null();
                        Expect(result.Id).To.Equal(expectedId);
                        Expect(result.Name).To.Equal(expectedName);
                    }

                    [Test]
                    public void ObjectAndConnectionStringCollection()
                    {
                        // Arrange
                        var expectedName = GetRandomString();
                        var expectedId = GetRandomInt();
                        var first = new
                        {
                            Id = expectedId
                        };
                        var second = new ConnectionStringSettingsCollection()
                        {
                            new ConnectionStringSettings(
                                "Name", expectedName
                            )
                        };
                        // Pre-Assert
                        // Act
                        var result = new object[] { first, second }.MergeAs<IMerge1>();
                        // Assert
                        Expect(result).Not.To.Be.Null();
                        Expect(result.Id).To.Equal(expectedId);
                        Expect(result.Name).To.Equal(expectedName);
                    }
                }
            }

            [TestFixture]
            public class Fuzzy
            {
                [TestFixture]
                public class OperatingOnCollectionOfOne
                {
                    [Test]
                    public void WhenCanBackTheInterface_ShouldReturnDuck()
                    {
                        // Arrange
                        var expectedId = GetRandomInt();
                        var expectedName = GetRandomString();
                        var src1 = new Dictionary<string, object>()
                        {
                            ["id"] = expectedId.ToString() /* et, voila! */,
                            ["Name"] = expectedName
                        };

                        // Act
                        var result = new[] {src1}.FuzzyMergeAs<IMerge1>();
                        // Assert
                        Expect(result).Not.To.Be.Null();
                        Expect(result.Id).To.Equal(expectedId);
                        Expect(result.Name).To.Equal(expectedName);
                    }

                    [Test]
                    public void WhenCannotBackTheInterface_ShouldReturnNull()
                    {
                        // Arrange
                        var src1 = new Dictionary<string, object>()
                        {
                            ["Id"] = Guid.NewGuid(),
                            ["Name"] = "Bob"
                        };

                        // Act
                        var result = new[] {src1}.FuzzyMergeAs<IMerge1>();
                        // Assert
                        Expect(result).To.Be.Null();
                    }

                    [Test]
                    public void WhenCannotBackTheInterfaceAndInstructedToThrow_ShouldThrow()
                    {
                        // Arrange
                        var src1 = new Dictionary<string, object>()
                        {
                            ["Identifier"] = 1,
                            ["Name"] = "Bob"
                        };

                        // Act
                        Expect(() => new[] {src1}.FuzzyMergeAs<IMerge1>(true))
                            .To.Throw<UnDuckableException>();
                        // Assert
                    }
                }

                [TestFixture]
                public class OperatingOnCollectionOfMoreThanOneWhichTogetherCanBackTheInterface
                {
                    [Test]
                    public void TwoObjects()
                    {
                        // Arrange
                        var first = new
                        {
                            id = GetRandomInt()
                        };
                        var second = new
                        {
                            Name = GetRandomString() // TODO: it would be nice if the fuzzy ducker could ignore, eg underscores here
                        };
                        // Pre-Assert
                        // Act
                        var result = new object[] {first, second}.FuzzyMergeAs<IMerge1>();
                        // Assert
                        Expect(result).Not.To.Be.Null();
                        Expect(result.Id).To.Equal(first.id);
                        Expect(result.Name).To.Equal(second.Name);
                    }

                    [Test]
                    public void DictionaryAndNameValueCollection()
                    {
                        // Arrange
                        var expectedId = GetRandomInt();
                        var expectedName = GetRandomString();
                        var first = new Dictionary<string, int>()
                        {
                            ["iD"] = expectedId
                        };
                        var second = new NameValueCollection()
                        {
                            {"nAmE", expectedName}
                        };
                        // Pre-Assert
                        // Act
                        var result = new object[] {first, second}.FuzzyMergeAs<IMerge1>();
                        // Assert
                        Expect(result).Not.To.Be.Null();
                        Expect(result.Id).To.Equal(expectedId);
                        Expect(result.Name).To.Equal(expectedName);
                    }

                    [Test]
                    public void DictionaryAndObject()
                    {
                        // Arrange
                        var expectedId = GetRandomInt();
                        var expectedName = GetRandomString();
                        var first = new Dictionary<string, object>()
                        {
                            ["iD"] = expectedId
                        };
                        var second = new
                        {
                            nAmE = expectedName
                        };
                        // Pre-Assert
                        // Act
                        var result = new object[] {first, second}.FuzzyMergeAs<IMerge1>();
                        // Assert
                        Expect(result).Not.To.Be.Null();
                        Expect(result.Id).To.Equal(expectedId);
                        Expect(result.Name).To.Equal(expectedName);
                    }

                    [Test]
                    public void ObjectAndConnectionStringCollection()
                    {
                        // Arrange
                        var expectedName = GetRandomString();
                        var expectedId = GetRandomInt();
                        var first = new
                        {
                            ID = expectedId
                        };
                        var second = new ConnectionStringSettingsCollection()
                        {
                            new ConnectionStringSettings(
                                "name", expectedName
                            )
                        };
                        // Pre-Assert
                        // Act
                        var result = new object[] { first, second }.FuzzyMergeAs<IMerge1>();
                        // Assert
                        Expect(result).Not.To.Be.Null();
                        Expect(result.Id).To.Equal(expectedId);
                        Expect(result.Name).To.Equal(expectedName);
                    }
                }
            }
        }

        public interface IMerge1
        {
            int Id { get; }
            string Name { get; }
        }
    }
}