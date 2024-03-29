using System.Linq;
using NExpect;
using NUnit.Framework;
using PeanutButter.TempDb.MySql.Base;
using PeanutButter.TempDb.MySql.Data;
using PeanutButter.Utils;
using PeanutButter.WindowsServiceManagement;
using static NExpect.Expectations;

namespace PeanutButter.TempDb.Tests
{
    [TestFixture]
    public class TestTempDbMySql
    {
        [Test]
        public void ShouldBeAbleToInitialize()
        {
            // Arrange
            // Act
            if (Platform.IsWindows)
            {
                SkipIfOnWindowsWithoutMySqlInstalled();
                var mysqld = MySqlWindowsServiceFinder.FindPathToMySqlD();
                Expect(mysqld).Not.To.Be.Null(
                    "Unable to find mysql service via sc command on this platform"
                );

                Expect(() =>
                    {
                        using (new TempDBMySql())
                        {
                        }
                    })
                    .Not.To.Throw();
            }
            else
            {
                Expect(() =>
                    {
                        using (new TempDBMySql())
                        {
                        }
                    })
                    .Not.To.Throw();
            }
        }

        private static void SkipIfOnWindowsWithoutMySqlInstalled()
        {
            if (!Platform.IsWindows)
            {
                Assert.Ignore("Tests require windows service infrastructure");
                return;
            }

            var mysqlServices =
#pragma warning disable CA1416
                new ServiceControlInterface().ListAllServices()
                .Where(s => s.ToLower().Contains("mysql"));
            if (!mysqlServices.Any())
            {
                Assert.Ignore(
                    "Test only works when there is at least one mysql service installed and that service has 'mysql' in the name (case-insensitive)"
                );
            }
#pragma warning restore CA1416
        }
    }
}