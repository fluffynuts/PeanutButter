using PeanutButter.Utils;

namespace PeanutButter.ServiceShell.NetCore.Tests
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