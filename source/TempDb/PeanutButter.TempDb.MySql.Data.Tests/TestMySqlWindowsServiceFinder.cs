using System.IO;
using NUnit.Framework;
using NExpect;
using PeanutButter.TempDb.MySql.Base;
using static NExpect.Expectations;

namespace PeanutButter.TempDb.MySql.Data.Tests
{
    [TestFixture]
    public class TestMySqlWindowsServiceFinder
    {
        const string EXPLICIT = "Requires Windows with an installation of mysql";

        [Test]
        [Explicit(EXPLICIT)]
        public void ShouldFindMySqlDWhenInstalled()
        {
            // Arrange
            // Act
            var result = MySqlWindowsServiceFinder.FindPathToMySqlD();
            // Assert
            Expect(result)
                .To.Exist();
        }

        [TestCase("mysql", "mysql.exe")]
        [TestCase("mysql.exe", "mysql.exe")]
        [TestCase("mysqldump", "mysqldump.exe")]
        [TestCase("mysqldump.exe", "mysqldump.exe")]
        [Explicit(EXPLICIT)]
        public void ShouldFindMySqlCli(
            string search,
            string expectedFilename
        )
        {
            // Arrange
            // Act
            var result = MySqlWindowsServiceFinder.FindPathTo("mysql");
            // Assert
            Expect(result)
                .To.Exist();
            Expect(Path.GetFileName(result).ToLower())
                .To.Equal("mysql.exe");
        }
    }
}