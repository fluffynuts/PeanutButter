using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace EmailSpooler.Win32Service.Tests
{
    [TestFixture]
    [Ignore]
    class TestEmailConfiguration
    {
        [Test]
        [Ignore("Run manually as this test requires an app.config and will break when running in multiple-assembly Resharper session")]
        public void CreateFromAppConfig_SetsPropertiesFromConfig()
        {
            // test setup
            
            // pre-conditions

            // execute test
            var config = EmailConfiguration.CreateFromAppConfig();

            // test result
            Assert.IsNotNull(config.UserName);
            Assert.AreEqual("TestUser", config.UserName);
            Assert.IsNotNull(config.Host);
            Assert.AreEqual("TestHost", config.Host);
            Assert.AreEqual(587, config.Port);
            Assert.IsTrue(config.SSLEnabled);
        }
    }
}
