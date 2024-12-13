using NUnit.Framework;
using PeanutButter.DuckTyping.Extensions;
using PeanutButter.DuckTyping.Shimming;

namespace PeanutButter.DuckTyping.Tests.Extensions;

public class ManualDuck : IHasOnlyAnId
{
    private ShimSham _shim;
    public int Id { get; }

    public ManualDuck(object[] toWrap)
    {
        _shim = new ShimSham(toWrap, typeof(IHasOnlyAnId), false, false);
    }
}

public interface IHasOnlyAnId
{
    int Id { get; }
}

[TestFixture]
public class TestDuckTypingCollectionExtensions
{
    [Test]
    public void DuckAs_GivenOneDuckableItemInCollection_ShouldDuckIt()
    {
        // Arrange
        var src = new[] {
            new {
                Id = 1
            }
        };

        // Pre-Assert

        // Act
        var result = src.DuckAsArrayOf<IHasOnlyAnId>();

        // Assert
        Expect(result)
            .Not.To.Be.Null();
        Expect(result[0].Id)
            .To.Equal(1);
    }

    [Test]
    public void DuckAs_GivenTwoDuckableItemInCollection_ShouldDuckIt()
    {
        // Arrange
        var src = new[] {
            new {
                Id = 1
            },
            new {
                Id = 2,
            }
        };

        // Pre-Assert

        // Act
        var result = src.DuckAsArrayOf<IHasOnlyAnId>();

        // Assert
        Expect(result)
            .Not.To.Be.Null();
        Expect(result)
            .To.Contain.Only(2).Items();
        Expect(result[0].Id)
            .To.Equal(1);
        Expect(result[1].Id)
            .To.Equal(2);
    }
}

