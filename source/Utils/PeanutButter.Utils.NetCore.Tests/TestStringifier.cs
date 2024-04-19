using PeanutButter.TestUtils.AspNetCore.Builders;

namespace PeanutButter.Utils.Tests;

[TestFixture]
public class TestStringifier
{
    [Test]
    public void ShouldStringifyPlainHttpContext()
    {
        // Arrange
        var ctx = HttpContextBuilder.BuildRandom();
        // Act
        var result = ctx.Stringify();
        // Assert
        var parts = result.Split(Stringifier.SEEN_OBJECT_PLACEHOLDER);
        Expect(parts)
            .To.Contain.At.Least(2)
            .Items(() => "httpcontext -> httprequest -> httpcontext should produce circular reference");
    }

    [Test]
    public void ShouldStringifyHttpContextReferencingItself()
    {
        // Arrange
        var ctx = HttpContextBuilder.BuildRandom();
        ctx.Items["context"] = ctx;
        // Act
        var result = ctx.Stringify();
        // Assert
        var parts = result.Split(Stringifier.SEEN_OBJECT_PLACEHOLDER);
        Expect(parts)
            .To.Contain.At.Least(3)
            .Items(
                () => $"result: {result}\nsplit on{Stringifier.SEEN_OBJECT_PLACEHOLDER}\nparts:\n{parts.JoinWith("- ")}"
            );
    }
}