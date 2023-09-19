using NUnit.Framework;
using PeanutButter.Utils;

namespace PeanutButter.ServiceShell.Tests
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