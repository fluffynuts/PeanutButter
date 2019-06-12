using System;
using NExpect;
using NUnit.Framework;
using PeanutButter.TempDb.MySql.Base;
using PeanutButter.TempDb.MySql.Data;
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
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                var mysqld = MySqlWindowsServiceFinder.FindPathToMySql();
                Expect(mysqld).Not.To.Be.Null(
                    "Unable to find mysql service via sc command on this platform"
                );

                Expect(() => new TempDBMySql())
                    .Not.To.Throw<FatalTempDbInitializationException>();
            }
            else
            {
                Expect(() => new TempDBMySql())
                    .To.Throw<FatalTempDbInitializationException>();
            }
        }
    }
}