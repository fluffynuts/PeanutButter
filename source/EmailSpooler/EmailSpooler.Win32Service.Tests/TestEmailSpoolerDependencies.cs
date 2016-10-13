using EmailSpooler.Win32Service.Entity;
using NSubstitute;
using NUnit.Framework;
using PeanutButter.ServiceShell;
using PeanutButter.TestUtils.Generic;

namespace EmailSpooler.Win32Service.Tests
{
    [TestFixture]
    public class TestEmailSpoolerDependencies
    {
        [Test]
        public void Construct_ShouldSetLoggerFromParameter()
        {
            //---------------Set up test pack-------------------
            var logger = Substitute.For<ISimpleLogger>();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = Create(logger);
            var result = sut.Logger;

            //---------------Test Result -----------------------
            Assert.AreEqual(logger, result);
        }

        [Test]
        public void Construct_ShouldSetupContext()
        {
            //---------------Set up test pack-------------------
            var sut = Create();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = sut.DbContext;

            //---------------Test Result -----------------------
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<EmailContext>(result);
        }

        [Test]
        public void Construct_ShouldSetEmailConfig()
        {
            //---------------Set up test pack-------------------
            var expected = EmailConfiguration.CreateFromAppConfig();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = Create();
            var result = sut.EmailConfig;

            //---------------Test Result -----------------------
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<EmailConfiguration>(result);
            PropertyAssert.AreDeepEqual(expected, result);
        }

        [Test]
        public void Construct_ShouldSetSpoolerConfig()
        {
            //---------------Set up test pack-------------------
            var logger = Substitute.For<ISimpleLogger>();
            var expected = new EmailSpoolerConfig(logger);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = Create(logger);
            var result = sut.EmailSpoolerConfig;

            //---------------Test Result -----------------------
            Assert.IsNotNull(expected);
            Assert.IsInstanceOf<EmailSpoolerConfig>(result);
            PropertyAssert.AreDeepEqual(expected, result);
        }


        [Test]
        public void Construct_ShouldSetupEmailGenerator()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = Create();
            var result = sut.EmailGenerator();

            //---------------Test Result -----------------------
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<SMTP.Email>(result);
            // ReSharper disable once PossibleNullReferenceException
            Assert.AreEqual(sut.EmailConfig, (result as SMTP.Email).EmailConfiguration);
        }


        private EmailSpoolerDependencies Create(ISimpleLogger logger = null)
        {
            return new EmailSpoolerDependencies(logger ?? Substitute.For<ISimpleLogger>());
        }
    }
}