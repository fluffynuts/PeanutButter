namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestAutoResettingFile
    {
        [TestFixture]
        public class AutomaticStorage
        {
            [Test]
            public void ShouldResetTextFile()
            {
                // Arrange
                var original = GetRandomWords();
                var updated = GetRandomWords();
                using var tempFile = new AutoTempFile();
                // Act
                File.WriteAllText(tempFile.Path, original);
                using (new AutoResettingFile(tempFile.Path))
                {
                    File.WriteAllText(tempFile.Path, updated);
                }

                // Assert
                var result = File.ReadAllText(tempFile.Path);
                Expect(result)
                    .To.Equal(original);
            }

            [Test]
            public void ShouldResetBinaryFile()
            {
                // Arrange
                var original = GetRandomBytes();
                var updated = GetRandomBytes();
                using var tempFile = new AutoTempFile();
                // Act
                File.WriteAllBytes(tempFile.Path, original);
                using (new AutoResettingFile(tempFile.Path))
                {
                    File.WriteAllBytes(tempFile.Path, updated);
                }

                // Assert
                var result = File.ReadAllBytes(tempFile.Path);
                Expect(result)
                    .To.Equal(original);
            }
        }

        [TestFixture]
        public class MemoryStorage
        {
            [Test]
            public void ShouldResetTextFile()
            {
                // Arrange
                var original = GetRandomWords();
                var updated = GetRandomWords();
                using var tempFile = new AutoTempFile();
                // Act
                File.WriteAllText(tempFile.Path, original);
                using (new AutoResettingFile(tempFile.Path, StorageTypes.InMemory))
                {
                    File.WriteAllText(tempFile.Path, updated);
                }

                // Assert
                var result = File.ReadAllText(tempFile.Path);
                Expect(result)
                    .To.Equal(original);
            }

            [Test]
            public void ShouldResetBinaryFile()
            {
                // Arrange
                var original = GetRandomBytes();
                var updated = GetRandomBytes();
                using var tempFile = new AutoTempFile();
                // Act
                File.WriteAllBytes(tempFile.Path, original);
                using (new AutoResettingFile(tempFile.Path, StorageTypes.InMemory))
                {
                    File.WriteAllBytes(tempFile.Path, updated);
                }

                // Assert
                var result = File.ReadAllBytes(tempFile.Path);
                Expect(result)
                    .To.Equal(original);
            }
        }

        [TestFixture]
        public class DiskStorage
        {
            [Test]
            public void ShouldResetTextFile()
            {
                // Arrange
                var original = GetRandomWords();
                var updated = GetRandomWords();
                using var tempFile = new AutoTempFile();
                // Act
                File.WriteAllText(tempFile.Path, original);
                using (new AutoResettingFile(tempFile.Path, StorageTypes.OnDisk))
                {
                    File.WriteAllText(tempFile.Path, updated);
                }

                // Assert
                var result = File.ReadAllText(tempFile.Path);
                Expect(result)
                    .To.Equal(original);
            }

            [Test]
            public void ShouldResetBinaryFile()
            {
                // Arrange
                var original = GetRandomBytes();
                var updated = GetRandomBytes();
                using var tempFile = new AutoTempFile();
                // Act
                File.WriteAllBytes(tempFile.Path, original);
                using (new AutoResettingFile(tempFile.Path, StorageTypes.OnDisk))
                {
                    File.WriteAllBytes(tempFile.Path, updated);
                }

                // Assert
                var result = File.ReadAllBytes(tempFile.Path);
                Expect(result)
                    .To.Equal(original);
            }
        }
    }
}