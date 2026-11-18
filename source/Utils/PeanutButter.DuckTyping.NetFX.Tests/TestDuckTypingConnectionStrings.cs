using System.Configuration;
using NUnit.Framework;
using PeanutButter.DuckTyping.Exceptions;
using PeanutButter.DuckTyping.Extensions;
using static NExpect.Expectations;
using NExpect;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.DuckTyping.NetFX.Tests;

public interface IConnectionStrings
{
    string SomeString { get; }
}

[TestFixture]
public class OperatingOnConnectionStringSettingsCollection
{
    [Test]
    public void WhenCanFuzzyDuck_ShouldFuzzyDuck()
    {
        // Arrange
        var expected = GetRandomString(2);
        var setting = new ConnectionStringSettings(
            "some string",
            expected
        );
        var settings = new ConnectionStringSettingsCollection
        {
            setting
        };
        // Pre-Assert
        // Act
        var result = settings.FuzzyDuckAs<IConnectionStrings>();
        // Assert
        Expect(result)
            .Not.To.Be.Null();
        Expect(result.SomeString)
            .To.Equal(expected);
    }

    [Test]
    public void
        WhenCantDuckBecauseStringPropertyIsMissing_GivenShouldThrow_ShouldNotThrow()
    {
        // Arrange
        var setting = new ConnectionStringSettings(
            "123some string",
            "some value"
        );
        var settings = new ConnectionStringSettingsCollection
        {
            setting
        };
        // Pre-Assert
        // Act
        Expect(() => settings.DuckAs<IConnectionStrings>(true))
            .To.Throw<UnDuckableException>();
        // Assert
    }

    [Test]
    public void WhenCanDuck_ShouldFuzzyDuck()
    {
        // Arrange
        var expected = GetRandomString(2);
        var setting = new ConnectionStringSettings(
            "SomeString",
            expected
        );
        var settings = new ConnectionStringSettingsCollection
        {
            setting
        };
        // Pre-Assert
        // Act
        var result = settings.DuckAs<IConnectionStrings>();
        // Assert
        Expect(result)
            .Not.To.Be.Null();
        Expect(result.SomeString)
            .To.Equal(expected);
    }

    [Test]
    public void WhenCannotDuck_GivenShouldThrow_ShouldThrow()
    {
        // Arrange
        var setting = new ConnectionStringSettings(
            "MooSomeString",
            "some value"
        );
        var settings = new ConnectionStringSettingsCollection
        {
            setting
        };
        // Pre-Assert
        // Act
        Expect(() => settings.DuckAs<IConnectionStrings>(true))
            .To.Throw<UnDuckableException>();
        // Assert
    }
}
