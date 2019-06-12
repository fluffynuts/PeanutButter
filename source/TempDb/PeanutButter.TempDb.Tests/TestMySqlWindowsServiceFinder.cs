using NUnit.Framework;
using NExpect;
using PeanutButter.TempDb.MySql.Base;
using PeanutButter.WindowsServiceManagement;
using static NExpect.Expectations;

namespace PeanutButter.TempDb.Tests
{
    [TestFixture]
    public class TestMySqlWindowsServiceFinder
    {
        [Test]
        public void ShouldBeAbleToFindServiceWhenNotRunning()
        {
            // Arrange
            var svc = new WindowsServiceUtil("mysql57");
            svc.Stop();
            // Act
            var path = MySqlWindowsServiceFinder.FindPathToMySql();
            // Assert
            Expect(path).Not.To.Be.Null.Or.Empty();
        }
    }
}