using PeanutButter.Utils;

namespace PeanutButter.WindowsServiceManagement.Core.Tests
{
    [SetUpFixture]
    public class GlobalSetup
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            if (!Platform.IsWindows)
            {
                Assert.Ignore("Windows-specific tests");
            }
        }
    }
}