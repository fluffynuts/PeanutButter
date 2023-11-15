using System;
using NUnit.Framework;
using NExpect;
using PeanutButter.TempDb.MySql.Base;
using PeanutButter.Utils;
using PeanutButter.WindowsServiceManagement;
using static NExpect.Expectations;

namespace PeanutButter.TempDb.Tests
{
    [TestFixture]
    public class TestMySqlServiceFinder
    {
        [Test]
        public void ShouldBeAbleToFindServiceWhenNotRunning()
        {
            // Arrange
            if (!Platform.IsWindows)
            {
                Assert.Ignore(
                    "Requires the service infrastructure of windows"
                );
                return;
            }

            var serviceName = MySqlWindowsServiceFinder.FindFirstMySqlServiceName();
            if (serviceName is null)
            {
                Assert.Ignore("Requires MySql to be installed as a service");
            }

            try
            {
                var svc = new WindowsServiceUtil(serviceName);
                svc.Stop();
            }
            catch (ServiceControlException ex)
            {
                if (ex.Message.Contains("access is denied", StringComparison.OrdinalIgnoreCase))
                {
                    Assert.Ignore("test requires admin privileges to run");
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(
                    $"Unable to stop mysql service '{serviceName}': {ex}"
                );
            }

            // Act
            var path = MySqlWindowsServiceFinder.FindPathToMySqlD();
            // Assert
            Expect(path)
                .Not.To.Be.Null.Or.Empty();
        }
    }
}