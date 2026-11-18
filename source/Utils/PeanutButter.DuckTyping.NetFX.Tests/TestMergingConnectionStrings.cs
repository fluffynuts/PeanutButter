using System.Configuration;
using NExpect;
using NUnit.Framework;
using PeanutButter.DuckTyping.Extensions;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.DuckTyping.NetFX.Tests;

[TestFixture]
public class TestMergingConnectionStrings
{
    public interface IMerge1
    {
        int Id { get; }
        string Name { get; }
    }

    [Test]
    public void CanMergeAs_WhenOperatingOnObjectAndConnectionStringsCollection_ShouldReturnTrue()
    {
        // Arrange
        var src1 = new
        {
            Id = 1
        };
        var src2 = new ConnectionStringSettingsCollection
        {
            new ConnectionStringSettings(
                "Name",
                "Bob"
            )
        };
        // Act
        var result = new object[]
        {
            src1,
            src2
        }.CanMergeAs<IMerge1>();
        // Assert
        Expect(result)
            .To.Be.True();
    }

    [Test]
    public void MergeAs_ShouldMergeObjectAndConnectionStringCollection()
    {
        // Arrange
        var expectedName = GetRandomString();
        var expectedId = GetRandomInt();
        var first = new
        {
            Id = expectedId
        };
        var second = new ConnectionStringSettingsCollection
        {
            new ConnectionStringSettings(
                "Name",
                expectedName
            )
        };
        // Act
        var result = new object[]
        {
            first,
            second
        }.MergeAs<IMerge1>();
        // Assert
        Expect(result)
            .Not.To.Be.Null();
        Expect(result.Id)
            .To.Equal(expectedId);
        Expect(result.Name)
            .To.Equal(expectedName);
    }

    [Test]
    public void FuzzyMergeAs_ShouldMergeObjectAndConnectionStringCollection()
    {
        // Arrange
        var expectedName = GetRandomString();
        var expectedId = GetRandomInt();
        var first = new
        {
            ID = expectedId
        };
        var second = new ConnectionStringSettingsCollection
        {
            new ConnectionStringSettings(
                "name",
                expectedName
            )
        };
        // Act
        var result = new object[]
        {
            first,
            second
        }.FuzzyMergeAs<IMerge1>();
        // Assert
        Expect(result)
            .Not.To.Be.Null();
        Expect(result.Id)
            .To.Equal(expectedId);
        Expect(result.Name)
            .To.Equal(expectedName);
    }
}
