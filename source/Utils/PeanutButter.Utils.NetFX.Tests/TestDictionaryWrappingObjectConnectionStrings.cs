using System;
using System.Configuration;
using PeanutButter.Utils.Dictionaries;

namespace PeanutButter.Utils.NetFX.Tests;

[TestFixture]
public class TestDictionaryWrappingObjectConnectionStrings
{
    [Test]
    public void ShouldBeAbleToReadAndWriteValue()
    {
        // Arrange
        var expected1 = GetRandomString();
        var expected2 = GetAnother(expected1);
        var wrapped = new ConnectionStringSettingsCollection
        {
            new ConnectionStringSettings(
                "id",
                expected1
            )
        };
        var sut = new DictionaryWrappingObject(
            wrapped,
            StringComparer.Ordinal,
            WrapOptions.None
        );
        // Act
        var result1 = sut["id"];
        sut["id"] = expected2;
        var result2 = sut["id"];
        // Assert
        Expect(result1)
            .To.Equal(expected1);
        Expect(result2)
            .To.Equal(expected2);
    }
}
