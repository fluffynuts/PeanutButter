namespace PeanutButter.Utils.Tests;

[TestFixture]
public class TestRunOnce
{
    [Test]
    public void ShouldRunTheActionOnlyOnce()
    {
        // Arrange
        var count = 0;
        var run = false;
        // Act
        Run.Once(ref run, () => count++);
        Run.Once(ref run, () => count++);
        // Assert
        Expect(count)
            .To.Equal(1);
    }
}