using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using NSubstitute;
using NUnit.Framework;
using EmailSpooler.Win32Service.SMTP;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;

namespace EmailSpooler.Win32Service.Tests
{
    [TestFixture]
    public class TestEmail
    {
        private class Email_ExposesInternals : Email
        {
            public Email_ExposesInternals()
                : base(Substitute.For<IEmailConfiguration>())
            {
            }
            public Email_ExposesInternals(IEmailConfiguration config)
                : base(config)
            {
            }
            public new bool DetermineIfBodyIsHTML()
            {
                return base.DetermineIfBodyIsHTML();
            }
            public new MailMessage CreateMessage()
            {
                return base.CreateMessage();
            }
            public List<IDisposable> Disposables { get { return _disposables; } }
            public void AddDisposable(IDisposable disposable)
            {
                this._disposables.Add(disposable);
            }
            public new SmtpClient CreateSMTPClient()
            {
                return base.CreateSMTPClient();
            }

        }
        private Email_ExposesInternals Create(IEmailConfiguration config = null)
        {
            return new Email_ExposesInternals(config ?? CreateRandomFallbackEmailConfiguration());
        }

        private static IEmailConfiguration CreateRandomFallbackEmailConfiguration()
        {
            var randomConfig = Substitute.For<IEmailConfiguration>();
            var host = RandomValueGen.GetRandomString();
            randomConfig.Host.Returns(host);
            randomConfig.Port.Returns(RandomValueGen.GetRandomInt(25, 1024));
            randomConfig.UserName.Returns(RandomValueGen.GetRandomString());
            randomConfig.Password.Returns(RandomValueGen.GetRandomString());
            return randomConfig;
        }

        private Email_ExposesInternals CreateWithRandomRecipientAndSender(IEmailConfiguration config = null)
        {
            var email = Create(config);
            email.To.Add(RandomValueGen.GetRandomEmail());
            email.From = RandomValueGen.GetRandomEmail();
            return email;
        }

        [Test]
        public void Construct_GivenNullConfiguration_ThrowsException()
        {
            //---------------Set up test pack-------------------
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<ArgumentNullException>(() => new Email(null));
            
            //---------------Test Result -----------------------
        }

        [Test]
        public void Implements_IDisposable()
        {
            //---------------Set up test pack-------------------
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.IsInstanceOf<IDisposable>(Create());

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_SetsUpToList()
        {
            //---------------Set up test pack-------------------
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.IsNotNull(Create().To);

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_SetsUpCCList()
        {
            //---------------Set up test pack-------------------
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.IsNotNull(Create().CC);

            //---------------Test Result -----------------------
        }

        [Test]
        public void DetermineIfBodyIsHTML_WhenBodyIsPlainText_ReturnsFalse()
        {
            //---------------Set up test pack-------------------
            using (var email = Create())
            {
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                email.Body = "Hello";

                //---------------Test Result -----------------------
                Assert.IsFalse(email.DetermineIfBodyIsHTML());
            }
        }

        [Test]
        public void DetermineIfBodyIsHTML_WhenBodyIsHTML_ReturnsFalse()
        {
            //---------------Set up test pack-------------------
            using (var email = Create())
            {
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                email.Body = "<html><head></head><body>Hello</body></html>";

                //---------------Test Result -----------------------
                Assert.IsTrue(email.DetermineIfBodyIsHTML());
            }
        }

        [Test]
        public void Send_WhenBodyIsNull_ThrowsException()
        {
            //---------------Set up test pack-------------------
            using (var email = Create())
            {
                email.Body = null;
                //---------------Assert Precondition----------------
                Assert.IsNull(email.Body);

                //---------------Execute Test ----------------------
                var ex = Assert.Throws<ArgumentException>(() => email.Send());

                //---------------Test Result -----------------------
                StringAssert.Contains("body cannot be null", ex.Message);
            }
        }

        [Test]
        public void Construct_SetsBodyToEmptyString()
        {
            //---------------Set up test pack-------------------
            using (var email = Create())
            {
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Assert.AreEqual("", email.Body);

                //---------------Test Result -----------------------
            }
        }

        [Test]
        public void Send_WhenRecipientsListIsEmpty_ThrowsException()
        {
            //---------------Set up test pack-------------------
            using (var email = Create())
            {

                //---------------Assert Precondition----------------
                Assert.AreEqual(0, email.To.Count);
                //---------------Execute Test ----------------------
                var ex = Assert.Throws<ArgumentException>(() => email.Send());

                //---------------Test Result -----------------------
                StringAssert.Contains("no recipients", ex.Message);
            }
        }

        [Test]
        public void Construct_SetsSubjectToEmptyString()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var email = Create();

            //---------------Test Result -----------------------
            Assert.AreEqual("", email.Subject);
        }

        [Test]
        public void Send_WhenSubjectIsNull_ThrowsException()
        {
            //---------------Set up test pack-------------------
            using (var email = Create())
            {
                email.Subject = null;
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var ex = Assert.Throws<ArgumentException>(() => email.Send());

                //---------------Test Result -----------------------
                StringAssert.Contains("subject cannot be null", ex.Message);
            }
        }

        [Test]
        public void Construct_SetsFromToEmptyString()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var email = Create();

            //---------------Test Result -----------------------
            Assert.AreEqual("", email.Subject);
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase(" ")]
        public void Send_WhenFromIsInvalid_ThrowsException(string from)
        {
            //---------------Set up test pack-------------------
            using (var email = Create())
            {
                email.To.Add(RandomValueGen.GetRandomEmail());
                email.From = from;
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var ex = Assert.Throws<ArgumentException>(() => email.Send());

                //---------------Test Result -----------------------
                StringAssert.Contains("sender cannot be empty", ex.Message);
            }
        }

        [TestCase("Body", "Body")]
        [TestCase("Subject", "Subject")]
        public void CreateMessage_SetsMailMessagePropertyFromOwnProperty(string ownProp, string mmProp)
        {
            //---------------Set up test pack-------------------
            using (var email = CreateWithRandomRecipientAndSender())
            {

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var msg = email.CreateMessage();

                //---------------Test Result -----------------------
                PropertyAssert.AreEqual(email, msg, ownProp, mmProp);
            }
        }

        [Test]
        public void CreateMessgae_SetsFromFromFrom() // !
        {
            //---------------Set up test pack-------------------
            using (var email = CreateWithRandomRecipientAndSender())
            {
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var message = email.CreateMessage();

                //---------------Test Result -----------------------
                Assert.AreEqual(message.From.Address, email.From);
            }
        }

        [Test]
        public void CreateMessgae_SetsSenderFromFrom() // !
        {
            //---------------Set up test pack-------------------
            using (var email = CreateWithRandomRecipientAndSender())
            {
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var message = email.CreateMessage();

                //---------------Test Result -----------------------
                Assert.AreEqual(message.Sender.Address, email.From);
            }
        }

        [Test]
        public void AddAttachment_AddsAttachmentToAttachmentsList()
        {
            //---------------Set up test pack-------------------
            using (var email = CreateWithRandomRecipientAndSender())
            {
                var fileName = RandomValueGen.GetRandomString();
                var data = RandomValueGen.GetRandomBytes();
                var mimeType = RandomValueGen.GetRandomMIMEType();
                //---------------Assert Precondition----------------
                Assert.AreEqual(0, email.Attachments.Count);
                //---------------Execute Test ----------------------
                email.AddAttachment(fileName, data, mimeType);

                //---------------Test Result -----------------------
                var attachment = email.Attachments.FirstOrDefault(a => a.Name == fileName && a.MIMEType == mimeType);
                Assert.IsNotNull(attachment);
                CollectionAssert.AreEqual(data, attachment.Data);
                var message = email.CreateMessage();

                var actual = message.Attachments.First();
                Assert.IsFalse(actual.ContentDisposition.Inline);
                Assert.AreEqual(actual.ContentDisposition.DispositionType, DispositionTypeNames.Attachment);
            }
        }

        [Test]
        public void AddAttachment_VariationWithContentID_AddsAttachmentToAttachmentsListWithProvidedContentID()
        {
            //---------------Set up test pack-------------------
            using (var email = CreateWithRandomRecipientAndSender())
            {
                var fileName = RandomValueGen.GetRandomString();
                var data = RandomValueGen.GetRandomBytes();
                var mimeType = RandomValueGen.GetRandomMIMEType();
                var contentID = RandomValueGen.GetRandomAlphaNumericString();

                //---------------Assert Precondition----------------
                Assert.AreEqual(0, email.Attachments.Count);
                //---------------Execute Test ----------------------
                email.AddAttachment(fileName, data, mimeType, contentID);

                //---------------Test Result -----------------------
                var attachment = email.Attachments.FirstOrDefault(a => a.Name == fileName && a.MIMEType == mimeType && a.ContentID == contentID);
                Assert.IsNotNull(attachment);
                CollectionAssert.AreEqual(data, attachment.Data);
                var message = email.CreateMessage();

                var actual = message.Attachments.First();
                Assert.IsTrue(actual.ContentDisposition.Inline);
                Assert.AreEqual(actual.ContentDisposition.DispositionType, DispositionTypeNames.Inline);
            }
        }

        [Test]
        public void AddAttachment_ReturnsIDForAttachment()
        {
            //---------------Set up test pack-------------------
            using (var email = CreateWithRandomRecipientAndSender())
            {
                var fileName = RandomValueGen.GetRandomString();
                var data = RandomValueGen.GetRandomBytes();
                var mimeType = "text/plain";
                email.Subject = RandomValueGen.GetRandomString();
                email.Body = RandomValueGen.GetRandomString();
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
                var id = email.AddAttachment(fileName, data, mimeType, true);
                var message = email.CreateMessage();
            //---------------Test Result -----------------------
                var att = message.Attachments.FirstOrDefault();
                Assert.IsNotNull(att);
                Assert.AreEqual(id, att.ContentId);
                Assert.IsTrue(att.ContentDisposition.Inline);
                Assert.AreEqual(att.ContentDisposition.DispositionType, DispositionTypeNames.Inline);
            }
        }

        [Test]
        public void AddPDFAttachment_AddsAttachmentWithMIMEType_APPLICATION_SLASH_PDF()
        {
            //---------------Set up test pack-------------------
            var fileName = RandomValueGen.GetRandomString();
            var bytes = RandomValueGen.GetRandomBytes();
            using (var email = CreateWithRandomRecipientAndSender())
            {
                //---------------Assert Precondition----------------
                Assert.IsFalse(email.Attachments.Any());
                //---------------Execute Test ----------------------
                email.AddPDFAttachment(fileName, bytes);

                //---------------Test Result -----------------------
                Assert.AreEqual(1, email.Attachments.Count);
                Assert.IsTrue(email.Attachments.All(a => a.Name == fileName && a.MIMEType == "application/pdf"));
            }
        }

        [Test]
        public void AddImageAttachment_AddsAttachmentWithMIMEType_FromExtensionAndReturnsAttachmentContentID()
        {
            // yes, I know MIME-from-extension is flaky. It's good enough for now.
            //---------------Set up test pack-------------------
            var fileName = RandomValueGen.GetRandomString() + ".png";
            var fileData = RandomValueGen.GetRandomBytes();

            using (var email = CreateWithRandomRecipientAndSender())
            {
                //---------------Assert Precondition----------------
                Assert.IsFalse(email.Attachments.Any());

                //---------------Execute Test ----------------------
                var result = email.AddInlineImageAttachment(fileName, fileData);

                //---------------Test Result -----------------------
                Assert.AreEqual(1, email.Attachments.Count);
                Assert.IsTrue(email.Attachments.All(a => a.Name == fileName && a.MIMEType == "image/png"));
                var message = email.CreateMessage();
                var attachment = message.Attachments.First();
                Assert.AreEqual(result, attachment.ContentId);
                Assert.IsTrue(attachment.ContentDisposition.Inline);
                Assert.AreEqual(DispositionTypeNames.Inline, attachment.ContentDisposition.DispositionType);
            }
        }

        [Test]
        public void Dispose_CallsDisposeOnAllDisposables()
        {
            //---------------Set up test pack-------------------
            var d1 = Substitute.For<IDisposable>();
            var d2 = Substitute.For<IDisposable>();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            using (var email = new Email_ExposesInternals())
            {
                email.AddDisposable(d1);
                email.AddDisposable(d2);
            }

            //---------------Test Result -----------------------
            d1.Received().Dispose();
            d2.Received().Dispose();
        }

        [Test]
        public void CreateMessage_AddsAttachmentMemoryStreamsToDisposables()
        {
            //---------------Set up test pack-------------------
            var fileName = RandomValueGen.GetRandomString();
            var data = RandomValueGen.GetRandomBytes();
            var mimeType = "text/plain";
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            using (var email = CreateWithRandomRecipientAndSender())
            {
                email.AddAttachment(fileName, data, mimeType);
                Assert.IsFalse(email.Disposables.Any());
                email.CreateMessage();
                //---------------Test Result -----------------------
                Assert.AreEqual(1, email.Disposables.Count(d => d as MemoryStream != null));
            }
        }

        [Test]
        public void CreateMessage_AddsMessageToDisposables()
        {
            //---------------Set up test pack-------------------
            using (var email = CreateWithRandomRecipientAndSender())
            {
                //---------------Assert Precondition----------------
                //---------------Execute Test ----------------------
                var message = email.CreateMessage();
                //---------------Test Result -----------------------
                Assert.IsTrue(email.Disposables.Any(d => d as MailMessage == message));
            }
        }

        [Test]
        public void CreateMessage_AddsAllAttachmentsToMailMessage()
        {
            //---------------Set up test pack-------------------
            var fileName = RandomValueGen.GetRandomString();
            var data = RandomValueGen.GetRandomBytes();
            var mimeType = "text/plain";
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            using (var email = CreateWithRandomRecipientAndSender())
            {
                email.AddAttachment(fileName, data, mimeType);
                var message = email.CreateMessage();
                //---------------Test Result -----------------------
                Assert.AreEqual(1, message.Attachments.Count);
            }
        }

        [Test]
        public void CreateSMTPClient_ReturnsSMTPClientInstance()
        {
            //---------------Set up test pack-------------------
            using (var email = CreateWithRandomRecipientAndSender())
            {
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var client = email.CreateSMTPClient();

                //---------------Test Result -----------------------
                Assert.IsNotNull(client);
                Assert.IsInstanceOf<SmtpClient>(client);
            }
        }

        private EmailConfiguration CreateRandomEmailConfig()
        {
            var host = RandomValueGen.GetRandomString();
            var port = RandomValueGen.GetRandomInt(20, 128);
            var user = RandomValueGen.GetRandomString();
            var pass = RandomValueGen.GetRandomString();
            var ssl = RandomValueGen.GetRandomBoolean();
            return new EmailConfiguration(host, port, user, pass, ssl);
        }

        [TestCase("Host", "Host")]
        [TestCase("Port", "Port")]
        [TestCase("SSLEnabled", "EnableSsl")]
        public void CreateSMTPClient_SetsClientPropertyFromConfigProperty(string configProp, string clientProp)
        {
            //---------------Set up test pack-------------------
            var config = CreateRandomEmailConfig();
            using (var email = CreateWithRandomRecipientAndSender(config))
            {
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var client = email.CreateSMTPClient();
                //---------------Test Result -----------------------
                PropertyAssert.AreEqual(config, client, configProp, clientProp);
            }
        }

        [Test]
        public void CreateSMTPClient_SetsUseDefaultCredentialsToFalse()
        {
            //---------------Set up test pack-------------------
            var config = CreateRandomEmailConfig();
            using (var email = CreateWithRandomRecipientAndSender(config))
            {
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var client = email.CreateSMTPClient();
                //---------------Test Result -----------------------
                Assert.IsFalse(client.UseDefaultCredentials);
            }
        }

        [Test]
        public void CreateSMPTClient_SetsNetworkCredentialsFromConfig()
        {
            //---------------Set up test pack-------------------
            var config = CreateRandomEmailConfig();
            using (var email = CreateWithRandomRecipientAndSender(config))
            {
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var client = email.CreateSMTPClient();
                //---------------Test Result -----------------------
                var creds = client.Credentials as NetworkCredential;
                Assert.IsNotNull(creds);
                Assert.AreEqual(config.UserName, creds.UserName);
                Assert.AreEqual(config.Password, creds.Password);
            }
        }

        private class Email_ForTestingSendOperation : Email
        {
            private MailMessage _mailMessage;
            private SmtpClientFacade _smtpClient;
            public string[] Calls { get { return _calls.ToArray(); } }
            private List<string> _calls = new List<string>();

            public MailMessage MailMessage { get { return _mailMessage; } }
            public SmtpClientFacade SmtpClient { get { return _smtpClient; } }

            public Email_ForTestingSendOperation(IEmailConfiguration config)
                : base(config)
            {
            }

            protected override void CheckEmailParameters()
            {
                _calls.Add("CheckEmailParameters");
            }

            protected override MailMessage CreateMessage()
            {
                _calls.Add("CreateMessage");
                _mailMessage = Substitute.For<MailMessage>();
                return _mailMessage;
            }

            protected override SmtpClientFacade CreateSMTPClient()
            {
                _calls.Add("CreateSMPTClient");
                _smtpClient = Substitute.For<SmtpClientFacade>();
                _smtpClient.When(c => c.Send(Arg.Any<MailMessage>())).Do(ci => { });
                return _smtpClient;
            }
        }

        [Test]
        public void Send_PerformsCorrectOperationsInOrder()
        {
            //---------------Set up test pack-------------------
            var config = CreateRandomEmailConfig();
            using (var email = new Email_ForTestingSendOperation(config))
            {
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                email.Send();

                //---------------Test Result -----------------------
                var calls = email.Calls;
                Assert.AreEqual("CheckEmailParameters", calls[0]);
                Assert.AreEqual("CreateMessage", calls[1]);
                Assert.AreEqual("CreateSMPTClient", calls[2]);

                email.SmtpClient.Received().Send(email.MailMessage);
            }
        }

        [Test]
        public void AddRecipient_AddsRecipientToToList()
        {
            //---------------Set up test pack-------------------
            using (var email = Create())
            {
                //---------------Assert Precondition----------------
                Assert.IsFalse(email.CC.Any());
                //---------------Execute Test ----------------------
                var randomEmail = RandomValueGen.GetRandomEmail();
                email.AddRecipient(randomEmail);
                //---------------Test Result -----------------------
                Assert.IsTrue(email.To.Any(t => t == randomEmail));
            }
        }
        [Test]
        public void AddCC_AddsCCToToList()
        {
            //---------------Set up test pack-------------------
            using (var email = Create())
            {
                //---------------Assert Precondition----------------
                Assert.IsFalse(email.To.Any());
                //---------------Execute Test ----------------------
                var randomEmail = RandomValueGen.GetRandomEmail();
                email.AddCC(randomEmail);
                //---------------Test Result -----------------------
                Assert.IsTrue(email.CC.Any(t => t == randomEmail));
            }
        }

        [Test]
        public void AddBCC_AddsBCCToToList()
        {
            //---------------Set up test pack-------------------
            using (var email = Create())
            {
                //---------------Assert Precondition----------------
                Assert.IsFalse(email.BCC.Any());
                //---------------Execute Test ----------------------
                var randomEmail = RandomValueGen.GetRandomEmail();
                email.AddBCC(randomEmail);
                //---------------Test Result -----------------------
                Assert.IsTrue(email.BCC.Any(t => t == randomEmail));
            }
        }

        [Test]
        public void CreateMessage_SetsRecipientsFromSelf()
        {
            //---------------Set up test pack-------------------
            using (var email = CreateWithRandomRecipientAndSender())
            {

                //---------------Assert Precondition----------------
                var address = email.To.First();               
                //---------------Execute Test ----------------------
                var message = email.CreateMessage();

                //---------------Test Result -----------------------
                Assert.IsTrue(email.To.All(a => message.To.Contains(new MailAddress(a))));
            }
        }

        [Test]
        public void CreateMessage_SetsCCListFromSelf()
        {
            //---------------Set up test pack-------------------
            using (var email = CreateWithRandomRecipientAndSender())
            {

                //---------------Assert Precondition----------------
                var cc = RandomValueGen.GetRandomEmail();
                email.AddCC(cc);

                //---------------Execute Test ----------------------
                var message = email.CreateMessage();

                //---------------Test Result -----------------------
                Assert.IsTrue(email.CC.All(a => message.CC.Contains(new MailAddress(cc))));
            }
        }

        [Test]
        public void CreateMessage_SetsBCCListFromSelf()
        {
            //---------------Set up test pack-------------------
            using (var email = CreateWithRandomRecipientAndSender())
            {

                //---------------Assert Precondition----------------
                var bcc = RandomValueGen.GetRandomEmail();
                email.AddBCC(bcc);

                //---------------Execute Test ----------------------
                var message = email.CreateMessage();

                //---------------Test Result -----------------------
                Assert.IsTrue(email.BCC.All(a => message.Bcc.Contains(new MailAddress(bcc))));
            }
        }

    }
}
