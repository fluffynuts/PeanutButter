using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace EmailSpooler.Win32Service.Tests
{
    [TestFixture]
    class TestEmailConfiguration
    {
        [Test]
        public void CreateFromAppConfig_SetsPropertiesFromConfig()
        {
            // test setup
            
            // pre-conditions

            // execute test
            var config = EmailConfiguration.CreateFromAppConfig();

            // test result
            Assert.IsNotNull(config.UserName);
            Assert.AreEqual("each.brrealestate@gmail.com", config.UserName);
            Assert.IsNotNull(config.Host);
            Assert.AreEqual("smtp.gmail.com", config.Host);
            Assert.AreEqual(587, config.Port);
            Assert.IsTrue(config.SSLEnabled);
        }

        [Test]
        [Ignore("Integration")]
        public void EmailConfiguredWithThisConfigSends()
        {
            // test setup
            var config = EmailConfiguration.CreateFromAppConfig();
            using (var email = new Email(config))
            {
                // pre-conditions
                
                // execute test
                email.From = config.UserName;
                email.To.Add("davydm@gmail.com");
                email.Subject = "Testing";
                email.Body = "<html><head></head><body><h3>This is a test</h3><p>Please do not adjust your email client.</p></body></html>";
                Assert.DoesNotThrow(() => email.Send());
                // test result
            }
        }
    }
}
