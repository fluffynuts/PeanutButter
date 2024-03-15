// ReSharper disable StringLiteralTypo
namespace PeanutButter.Utils.Tests;

[TestFixture]
public class FilePathResolverTests
{
    [TestFixture]
    public class GivenPathToExistingFile
    {
        [Test]
        public void ShouldReturnThatPathOnly()
        {
            // Arrange
            using var tempFile = new AutoTempFile();
            var sut = Create();
            // Act
            var result = sut.Resolve(tempFile.Path);
            // Assert
            Expect(result)
                .To.Equal([tempFile.Path]);
        }
    }

    [TestFixture]
    public class GivenPathToFolder
    {
        [Test]
        public void ShouldEnumerateTheFolder()
        {
            // Arrange
            var fileNames = GetRandomArray<string>(3, 6);
            using var tempFolder = new AutoTempFolder();
            foreach (var item in fileNames)
            {
                tempFolder.WriteFile(item, GetRandomWords());
            }

            var expected = tempFolder.ResolvePaths(fileNames);
            var sut = Create();
            // Act
            var result = sut.Resolve(tempFolder.Path);
            // Assert
            Expect(result)
                .To.Be.Equivalent.To(expected);
        }

        [Test]
        public void ShouldRecurseTheFolder()
        {
            // Arrange
            using var tempFolder = new AutoTempFolder();
            var paths = GetRandomArray(GetRandomPath);
            foreach (var item in paths)
            {
                tempFolder.WriteFile(item, GetRandomWords());
            }

            var expected = tempFolder.ResolvePaths(paths);
            var sut = Create();
            // Act
            var result = sut.Resolve(tempFolder.Path);
            // Assert
            Expect(result)
                .To.Be.Equivalent.To(expected);
        }
    }

    [TestFixture]
    public class GivenGlob
    {
        [Test]
        public void ShouldReturnMatchingEntriesOnly1()
        {
            // Arrange
            var expected1 = "foobar.txt";
            var unexpected1 = "notes/oofoo.txt";
            var unexpected2 = "cowsay.md";
            using var tempFolder = new AutoTempFolder();
            tempFolder.WriteFile(expected1, GetRandomWords());
            tempFolder.WriteFile(unexpected1, GetRandomWords());
            tempFolder.WriteFile(unexpected2, GetRandomWords());
            var expected = tempFolder.ResolvePaths(
                new[] { expected1 }
            );
            var sut = Create();
            var input = Path.Combine(tempFolder.Path, "foo*");
            // Act
            var result = sut.Resolve(input);
            // Assert
            Expect(result)
                .To.Be.Equivalent.To(expected);
        }

        [Test]
        public void ShouldReturnMatchingEntriesOnly2()
        {
            // Arrange
            var expected1 = "foobar.txt";
            var expected2 = "notes/oofoo.txt";
            var unexpected = "cowsay.md";
            using var tempFolder = new AutoTempFolder();
            tempFolder.WriteFile(expected1, GetRandomWords());
            tempFolder.WriteFile(expected2, GetRandomWords());
            tempFolder.WriteFile(unexpected, GetRandomWords());
            var expected = tempFolder.ResolvePaths(
                new[] { expected1, expected2 }
            );
            var sut = Create();
            // Act
            var result = sut.Resolve(tempFolder.ResolvePath("*foo*"));
            // Assert
            Expect(result)
                .To.Be.Equivalent.To(expected);
        }

        [Test]
        public void ShouldBeAbleToFindMatchingEntriesWithGlobInDirectoryStructure()
        {
            // Arrange
            using var tempFolder = new AutoTempFolder();
            tempFolder.WriteFile("foo.txt", GetRandomWords());
            tempFolder.WriteFile("sub1/foo.txt", GetRandomWords());
            var expected = tempFolder.WriteFile("sub1/sub2/foo.txt", GetRandomWords());
            var sut = Create();
            // Act
            var result = sut.Resolve(
                tempFolder.ResolvePath("sub1/*/foo.txt")
            );
            // Assert
            Expect(result)
                .To.Equal([expected]);
        }
    }

    private static IFilePathResolver Create()
    {
        return new FilePathResolver();
    }
}
