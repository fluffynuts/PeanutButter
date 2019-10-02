using System;
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
            if(Platform.IsWindows)
            {
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
                .Not.To.Throw<FatalTempDbInitializationException>();
            }
            else
            {
                Expect(() =>
                    {
                        using (new TempDBMySql())
                        {
                        }
                    })
                    .To.Throw<FatalTempDbInitializationException>();
            }
        }
    }
}