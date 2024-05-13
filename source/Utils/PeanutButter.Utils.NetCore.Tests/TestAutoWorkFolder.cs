namespace PeanutButter.Utils.Tests;

[TestFixture]
public class TestAutoWorkFolder
{
    [Test]
    public void ShouldSetCurrentDirectoryDuringLifetime()
    {
        // Arrange
        using var tempFolder = new AutoTempFolder();
        var startDir = Environment.CurrentDirectory;
        Expect(startDir)
            .Not.To.Equal(tempFolder.Path);
        // Act
        using (var _ = Create(tempFolder))
        {
            Expect(Environment.CurrentDirectory)
                .To.Equal(tempFolder.Path);
        }

        // Assert
        Expect(Environment.CurrentDirectory)
            .To.Equal(startDir);
    }

    private static AutoWorkFolder Create(
        AutoTempFolder folder = null
    )
    {
        var shouldDisposeFolder = folder is null;
        return new AutoWorkFolder(folder, shouldDisposeFolder);
    }
}