using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.ServiceShell;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils;

namespace EmailSpooler.Win32Service.Tests
{
    [TestFixture]
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

    [TestFixture]
    public class TestEmailSpoolerConfig
    {
        [Test]
        public void Type_ShouldImplement_IEmailSpoolerConfig()
        {
            //---------------Set up test pack-------------------
            var sut = typeof (EmailSpoolerConfig);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldImplement<IEmailSpoolerConfig>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_GivenNullLogger_ShouldThrowANE()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ex = Assert.Throws<ArgumentNullException>(() => new EmailSpoolerConfig(null));

            //---------------Test Result -----------------------
            Assert.AreEqual("logger", ex.ParamName);
        }

        [Test]
        public void Construct_ShouldSetLoggerFromParameter()
        {
            //---------------Set up test pack-------------------
            var logger = Substitute.For<ISimpleLogger>();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = Create(logger);

            //---------------Test Result -----------------------
            Assert.AreEqual(logger, sut.Logger);
        }

        [TestCase("MaxSendAttempts")]
        [TestCase("BackoffIntervalInMinutes")]
        [TestCase("BackoffMultiplier")]
        [TestCase("PurgeMessageWithAgeInDays")]
        public void Construct_ShouldSetPropertyFromAppSettingsValue_(string settingName)
        {
            //---------------Set up test pack-------------------
            var settings = CreateRandomSettings();
            var expected = settings[settingName];
            var sut = CreateWithSettings(settings:settings);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = sut.GetPropertyValue<int>(settingName);

            //---------------Test Result -----------------------
            Assert.AreEqual(Convert.ToInt32(expected), result);
        }

        [TestCase("MaxSendAttempts", 5)]
        [TestCase("BackoffIntervalInMinutes", 2)]
        [TestCase("BackoffMultiplier", 2)]
        [TestCase("PurgeMessageWithAgeInDays", 30)]
        public void Construct_WhenConfiguredValueMissing_ShouldLogAndFallBackOnDefault(string property, int defaultValue)
        {
            //---------------Set up test pack-------------------
            var settings = new NameValueCollection();
            var expectedMessage = $"No configured value for '{property}'; falling back on default value: '{defaultValue}'";
            var logger = Substitute.For<ISimpleLogger>();

            //---------------Assert Precondition----------------
            Assert.IsNull(settings[property]);

            //---------------Execute Test ----------------------
            var sut = CreateWithSettings(logger, settings);

            //---------------Test Result -----------------------
            Assert.AreEqual(defaultValue, sut.GetPropertyValue<int>(property));
            logger.Received().LogInfo(expectedMessage);
        }

        [TestCase("MaxSendAttempts")]
        [TestCase("BackoffIntervalInMinutes")]
        [TestCase("BackoffMultiplier")]
        [TestCase("PurgeMessageWithAgeInDays")]
        public void Construct_RegularConstructor_ShouldUseAppSettings(string propertyName)
        {
            //---------------Set up test pack-------------------
            var expected = Convert.ToInt32(ConfigurationManager.AppSettings[propertyName]);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = Create(Substitute.For<ISimpleLogger>());
            var result = sut.GetPropertyValue<int>(propertyName);

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }


        private NameValueCollection CreateRandomSettings()
        {
            var result = new NameValueCollection();
            Func<string> randInt = () => RandomValueGen.GetRandomInt().ToString();
            result["MaxSendAttempts"] = randInt();
            result["BackoffIntervalInMinutes"] = randInt();
            result["BackoffMultiplier"] = randInt();
            result["PurgeMessageWithAgeInDays"] = randInt();
            return result;
        }



        private EmailSpoolerConfig Create(ISimpleLogger logger)
        {
            return new EmailSpoolerConfig(logger);
        }

        private EmailSpoolerConfig CreateWithSettings(ISimpleLogger logger = null, NameValueCollection settings = null)
        {
            var result = new EmailSpoolerConfig(logger ?? Substitute.For<ISimpleLogger>(), settings);
            Assert.IsInstanceOf<EmailSpoolerConfig>(result);
            return result;
        }
    }
}
