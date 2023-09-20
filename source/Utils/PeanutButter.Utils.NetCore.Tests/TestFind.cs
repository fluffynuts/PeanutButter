using NUnit.Framework;
using static NExpect.Expectations;
using NExpect;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestFind
    {
        [Test]
        public void ShouldBeAbleToFindNotePad()
        {
            if (!Platform.IsWindows)
            {
                Assert.Ignore("Looking for notepad.exe on !windows is unlikely to succeed");
                return;
            }

            // Arrange
            var search = "notepad";
            var expected = "C:\\Windows\\system32\\notepad.EXE"; // capitalization of extension is thanks to PATHEXT
            // Act
            var result = Find.InPath(search);
            // Assert
            Expect(result.ToLowerInvariant()).To.Equal(expected.ToLowerInvariant());
        }
        
        [Test]
        public void ShouldBeAbleToFindNoteExe()
        {
            if (!Platform.IsWindows)
            {
                Assert.Ignore("Looking for notepad.exe on !windows is unlikely to succeed");
                return;
            }

            // Arrange
            var search = "notepad.exe";
            var expected = "C:\\Windows\\system32\\notepad.EXE"; // capitalization of extension is thanks to PATHEXT
            // Act
            var result = Find.InPath(search);
            // Assert
            Expect(result.ToLowerInvariant()).To.Equal(expected.ToLowerInvariant());
        }
        
        [Test]
        [Explicit("requires local redis-server")]
        public void ShouldBeAbleToFindRedisOnOsx()
        {
            if (!Platform.IsUnixy)
            {
                Assert.Ignore("Looking for redis-server on windows will probably fail: relies on it being in the PATH.");
                return;
            }

            // Arrange
            var search = "redis-server";
            // Act
            var result = Find.InPath(search);
            // Assert
            Expect(result)
                .Not.To.Be.Null();
            Expect(result)
                .To.Exist();
        }
    }
}