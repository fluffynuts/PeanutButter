using Microsoft.Extensions.Primitives;
using NExpect;
using PeanutButter.TestUtils.AspNetCore.Builders;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using PeanutButter.Utils;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TestUtils.AspNetCore.Tests;

[TestFixture]
public class TestFakeFormBuilder
{
    [TestFixture]
    public class DefaultBuild
    {
        [Test]
        public void ShouldProduceForm()
        {
            // Arrange
            // Act
            var result = FakeFormBuilder.BuildDefault();
            // Assert
            Expect(result)
                .Not.To.Be.Null();
            Expect(result.Keys)
                .To.Be.Empty();
        }
    }

    [Test]
    public void ShouldBeAbleToAddFields()
    {
        // Arrange
        var field1 = GetRandomString(10);
        var value1 = GetRandomString();
        var field2 = GetRandomString(10);
        var value2 = GetRandomString();
        var expected = new Dictionary<string, StringValues>()
        {
            [field1] = value1,
            [field2] = value2
        };
        // Act
        var result = FakeFormBuilder.Create()
            .WithField(field1, value1)
            .WithField(field2, value2)
            .Build();
        // Assert
        Expect(result.FormValues)
            .To.Deep.Equal(expected);
    }

    [Test]
    public void ShouldBeAbleToAddFiles()
    {
        // Arrange
        var name = GetRandomString(10);
        var fileName = GetRandomFileName();
        var contents = GetRandomWords();
        
        // Act
        var result = FakeFormBuilder.Create()
            .WithFile(contents, name, fileName)
            .Build();
        // Assert
        Expect(result.Files.Count)
            .To.Equal(1);
        var file = result.Files[0];
        Expect(file.Name)
            .To.Equal(name);
        Expect(file.FileName)
            .To.Equal(fileName);
        using var s = file.OpenReadStream();
        Expect(s.ReadAllText())
            .To.Equal(contents);
    }

    [TestFixture]
    public class RandomFormCollection
    {
        [Test]
        public void ShouldHaveAtLeastOneFieldAndNoFiles()
        {
            // Arrange
            // force registration for a once-off run
            Register.RandomGenerators();
            // Act
            var result = GetRandom<FakeFormCollection>();
            // Assert
            Expect(result.Files.Count)
                .To.Equal(0);
            Expect(result.FormValues.Keys)
                .Not.To.Be.Empty();
        }
    }
}