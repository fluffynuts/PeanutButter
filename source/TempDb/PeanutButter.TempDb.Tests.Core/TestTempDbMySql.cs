using System.Linq;
using System.ServiceProcess;
using NExpect;
using NUnit.Framework;
using PeanutButter.TempDb.MySql.Base;
using PeanutButter.TempDb.MySql.Data;
using PeanutButter.Utils;
using static NExpect.Expectations;

namespace PeanutButter.TempDb.Tests.Core
{
    public class TestTempDbMySql
    {
        [Test]
        public void ShouldBeAbleToInitializeOnWindows()
        {
            // Arrange
            // Act
            if (Platform.IsWindows)
            {
                SkipIfOnWindowsWithoutMySqlInstalled();
                var mysqld = MySqlWindowsServiceFinder.FindPathToMySql();
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

        private static void SkipIfNotOnWindowsAndMySqlNotInPath()
        {
            if (Platform.IsWindows)
            {
                return;
            }

            var mysqld = Find.InPath("mysqld");
            if (mysqld is null)
            {
                Assert.Ignore(
                    "Test only works when mysqld is in your PATH"
                );
            }
        }

        private static void SkipIfOnWindowsWithoutMySqlInstalled()
        {
            if (!Platform.IsWindows)
            {
                return;
            }

            var mysqlServices =
                ServiceController.GetServices().Where(s => s.DisplayName.ToLower().Contains("mysql"));
            if (!mysqlServices.Any())
            {
                Assert.Ignore(
                    "Test only works when there is at least one mysql service installed and that service has 'mysql' in the name (case-insensitive)"
                );
            }
        }
    }
}