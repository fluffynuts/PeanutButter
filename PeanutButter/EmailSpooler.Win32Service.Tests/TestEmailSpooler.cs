using System;
using System.Collections.Generic;
using System.Linq;
using EmailSpooler.Win32Service.DB.Entities;
using EmailSpooler.Win32Service.Tests.Builders;
using NSubstitute;
using NUnit.Framework;
using EmailSpooler.Win32Service.SMTP;
using PeanutButter.RandomGenerators;
using PeanutButter.ServiceShell;
using PeanutButter.TestUtils.Generic;
using EmailAttachment = EmailSpooler.Win32Service.SMTP.EmailAttachment;

namespace EmailSpooler.Win32Service.Tests
{
    [TestFixture]
    public class TestEmailSpooler
    {
        private IEmail EmailGenerator()
        {
            return CreateSubstituteEmail();
        }

        private EmailSpooler Create(IEmailContext ctx, Func<IEmail> emailGenerator = null)
        {
            var deps = Substitute.For<IEmailSpoolerDependencies>();
            deps.EmailGenerator.ReturnsForAnyArgs(ci => emailGenerator ?? EmailGenerator);
            deps.DbContext.ReturnsForAnyArgs(ci => ctx);
            return new EmailSpooler(deps);
        }

        public class IFakeEmailSpoolerDependenciesBuilder: BuilderBase<IFakeEmailSpoolerDependenciesBuilder, IEmailSpoolerDependencies>, IBuilder<IEmailSpoolerDependencies>
        {
            public IFakeEmailSpoolerDependenciesBuilder()
            {
                var emailSpoolerConfig = Substitute.For<IEmailSpoolerConfig>();
                emailSpoolerConfig.MaxSendAttempts.ReturnsForAnyArgs(ci => 5);
                emailSpoolerConfig.BackoffMultiplier.ReturnsForAnyArgs(ci => 2);
                this.WithEmailSpoolerConfig(emailSpoolerConfig)
                    .WithDbContext(SubstituteEmailContextBuilder.BuildDefault())
                    .WithEmailConfig(Substitute.For<IEmailConfiguration>())
                    .WithSimpleLogger(Substitute.For<ISimpleLogger>())
                    .WithEmailGenerator(() => Substitute.For<IEmail>());
            }

            public IEmailSpoolerDependencies Build()
            {
                var deps = Substitute.For<IEmailSpoolerDependencies>();
                deps.EmailGenerator.ReturnsForAnyArgs(ci => this._EmailGenerator);
                deps.DbContext.ReturnsForAnyArgs(ci => this._DbContext);
                deps.EmailSpoolerConfig.ReturnsForAnyArgs(ci => this._Config);
                deps.EmailConfig.ReturnsForAnyArgs(ci => this._EmailConfig);
                return deps;
            }
            private Func<IEmail> _EmailGenerator;
            public IFakeEmailSpoolerDependenciesBuilder WithEmailGenerator(Func<IEmail> EmailGenerator)
            {
                this._EmailGenerator = EmailGenerator;
                return this;
            }
            private IEmailContext _DbContext;
            public IFakeEmailSpoolerDependenciesBuilder WithDbContext(IEmailContext DbContext)
            {
                this._DbContext = DbContext;
                return this;
            }
            private IEmailSpoolerConfig _Config;
            public IFakeEmailSpoolerDependenciesBuilder WithEmailSpoolerConfig(IEmailSpoolerConfig Config)
            {
                this._Config = Config;
                return this;
            }
            private IEmailConfiguration _EmailConfig;
            public IFakeEmailSpoolerDependenciesBuilder WithEmailConfig(IEmailConfiguration EmailConfig)
            {
                this._EmailConfig = EmailConfig;
                return this;
            }
            private ISimpleLogger _SimpleLogger;
            public IFakeEmailSpoolerDependenciesBuilder WithSimpleLogger(ISimpleLogger SimpleLogger)
            {
                this._SimpleLogger = SimpleLogger;
                return this;
            }

        }

        [Test]
        public void Construct_WhenGivenNullDBContext_ThrowsArgumentNullException()
        {
            // test setup
            var deps = IFakeEmailSpoolerDependenciesBuilder.Create().WithDbContext(null).Build();
            // pre-conditions

            // execute test
            var ex = Assert.Throws<ArgumentNullException>(() => new EmailSpooler(deps));

            // test result
            StringAssert.Contains("dbContext", ex.Message);
        }

        [Test]
        public void Construct_WhenGivenNullEmailGenerator_ThrowsArgumentNullException()
        {
            // test setup
                var deps = IFakeEmailSpoolerDependenciesBuilder.Create().WithEmailGenerator(null).Build();
            
            // pre-conditions

            // execute test
            var ex = Assert.Throws<ArgumentNullException>(() => new EmailSpooler(deps));

            // test result
            StringAssert.Contains("emailGenerator", ex.Message);
        }

        [Test]
        public void Construct_WhenGivenNullEmailSpoolerConfig_ThrowsArgumentNullException()
        {
            // test setup
            var deps = IFakeEmailSpoolerDependenciesBuilder.Create().WithEmailSpoolerConfig(null).Build();
            
            // pre-conditions

            // execute test
            var ex = Assert.Throws<ArgumentNullException>(() => new EmailSpooler(deps));

            // test result
            StringAssert.Contains("emailSpoolerConfig", ex.Message);
        }

        [Test]
        public void Dispose_DisposesDbContext()
        {
            // test setup
            var deps = IFakeEmailSpoolerDependenciesBuilder.BuildDefault();
            var ctx = deps.DbContext;

            // pre-conditions
            ctx.DidNotReceive().Dispose();

            // execute test
            using (new EmailSpooler(deps)) { }

            // test result
            ctx.Received().Dispose();
        }

        [Test]
        public void Spool_WhenNoEmailsInContext_DoesNothing()
        {
            // test setup
            var generatedMails = 0;
            Func<IEmail> emailGen = () =>
                {
                    generatedMails++;
                    return Substitute.For<IEmail>();
                };
            var ctx = SubstituteEmailContextBuilder.BuildDefault();
            var spooler = Create(ctx);
            
            // pre-conditions
            Assert.AreEqual(0, ctx.Emails.Count());

            // execute test
            spooler.Spool();

            // test result
            ctx.DidNotReceive().SaveChanges();
            Assert.AreEqual(0, generatedMails);
        }

        Func<IEmail> EmailGeneratorWith(params IEmail[] emails)
        {
            var queue = new Queue<IEmail>(emails);
            return queue.Dequeue;
        }

        private IEmail CreateSubstituteEmail()
        {
            var email = Substitute.For<IEmail>();
            var toList = new List<string>();
            email.To.ReturnsForAnyArgs(ci => toList);
            var ccList = new List<string>();
            email.CC.ReturnsForAnyArgs(ci => ccList);
            var bccList = new List<string>();
            email.BCC.ReturnsForAnyArgs(ci => bccList);
            var attachmentList = new List<EmailAttachment>();
            email.Attachments.ReturnsForAnyArgs(ci => attachmentList);
            return email;
        }


        [TestCase("Sender", "From")]
        [TestCase("Subject", "Subject")]
        [TestCase("Body", "Body")]
        public void Spool_WhenOneEmailNotSent_CreatesEmailWithPropertiesFromDTO(string dtoProp, string emailProp)
        {
            // test setup
            var email = CreateSubstituteEmail();
            var func = EmailGeneratorWith(email);
            var dto = EmailBuilder.BuildRandomWithRecipient();
            var ctx = SubstituteEmailContextBuilder.Create().WithEmails(dto).Build();
            var deps = IFakeEmailSpoolerDependenciesBuilder.Create()
                        .WithDbContext(ctx)
                        .WithEmailGenerator(func)
                        .Build();
            // pre-conditions
            Assert.IsTrue(dto.EmailRecipients.Any());

            // execute test
            using (var spooler = new EmailSpooler(deps))
            {
                spooler.Spool();
            }

            // test result
            PropertyAssert.AreEqual(dto, email, dtoProp, emailProp);
        }

        [Test]
        public void Spool_WhenOneEmailNotSent_CreatesEmailWithAllConfiguredRecipients()
        {
            // test setup
            var email = CreateSubstituteEmail();
            var func = EmailGeneratorWith(email);
            var dto = EmailBuilder.Create().WithRandomProps().WithRandomRecipient(2).Build();
            foreach (var recipient in dto.EmailRecipients)
            {
                recipient.IsPrimaryRecipient = true;
                recipient.IsCC = false;
                recipient.IsBCC = false;
            }
            var ctx = SubstituteEmailContextBuilder.Create().WithEmails(dto).Build();
            var deps = IFakeEmailSpoolerDependenciesBuilder.Create()
                        .WithDbContext(ctx)
                        .WithEmailGenerator(func)
                        .Build();
            // pre-conditions
            Assert.IsTrue(dto.EmailRecipients.Count(r => r.IsPrimaryRecipient && !r.IsCC && !r.IsBCC) == 2);

            // execute test
            using (var spooler = new EmailSpooler(deps))
            {
                spooler.Spool();
            }

            // test result
            foreach (var recipient in dto.EmailRecipients)
            {
                email.Received().AddRecipient(recipient.Recipient);
                email.DidNotReceive().AddCC(recipient.Recipient);
                email.DidNotReceive().AddBCC(recipient.Recipient);
            }
        }

        [Test]
        public void Spool_WhenOneEmailNotSent_CreatesEmailWithAllConfiguredCCs()
        {
            // test setup
            var email = CreateSubstituteEmail();
            var func = EmailGeneratorWith(email);
            var dto = EmailBuilder.Create().WithRandomProps().WithRandomCC(2).Build();
            var ctx = SubstituteEmailContextBuilder.Create().WithEmails(dto).Build();
            var deps = IFakeEmailSpoolerDependenciesBuilder.Create()
                        .WithDbContext(ctx)
                        .WithEmailGenerator(func)
                        .Build();
            // pre-conditions
            Assert.IsTrue(dto.EmailRecipients.Count(e => e.IsCC && !e.IsPrimaryRecipient && !e.IsBCC) > 1);

            // execute test
            using (var spooler = new EmailSpooler(deps))
            {
                spooler.Spool();
            }

            // test result
            foreach (var recipient in dto.EmailRecipients)
            {
                email.DidNotReceive().AddRecipient(recipient.Recipient);
                email.Received().AddCC(recipient.Recipient);
                email.DidNotReceive().AddBCC(recipient.Recipient);
            }
        }

        [Test]
        public void Spool_WhenOneEmailNotSent_CreatesEmailWithAllConfiguredBCCs()
        {
            // test setup
            var email = CreateSubstituteEmail();
            var func = EmailGeneratorWith(email);
            var dto = EmailBuilder.Create().WithRandomProps().WithRandomBCC(2).Build();
            var ctx = SubstituteEmailContextBuilder.Create().WithEmails(dto).Build();
            var deps = IFakeEmailSpoolerDependenciesBuilder.Create()
                        .WithDbContext(ctx)
                        .WithEmailGenerator(func)
                        .Build();
            // pre-conditions
            Assert.IsTrue(dto.EmailRecipients.Count(e => e.IsBCC && !e.IsPrimaryRecipient && !e.IsCC) > 1);

            // execute test
            using (var spooler = new EmailSpooler(deps))
            {
                spooler.Spool();
            }

            // test result
            foreach (var recipient in dto.EmailRecipients)
            {
                email.DidNotReceive().AddRecipient(recipient.Recipient);
                email.Received().AddBCC(recipient.Recipient);
                email.DidNotReceive().AddCC(recipient.Recipient);
            }
        }

        [Test]
        public void Spool_WhenOneEmailNotSent_AddsRecipientsToAllConfiguredFields()
        {
            // test setup
            var email = CreateSubstituteEmail();
            var func = EmailGeneratorWith(email);
            var dto = EmailBuilder.Create().WithRandomProps().WithRandomRecipient().Build();
            dto.EmailRecipients.First().IsBCC = true;
            dto.EmailRecipients.First().IsCC = true;
            var ctx = SubstituteEmailContextBuilder.Create().WithEmails(dto).Build();
            var deps = IFakeEmailSpoolerDependenciesBuilder.Create()
                        .WithDbContext(ctx)
                        .WithEmailGenerator(func)
                        .Build();
            // pre-conditions
            Assert.IsTrue(dto.EmailRecipients.Count(e => e.IsBCC && e.IsPrimaryRecipient && e.IsCC) == 1);

            // execute test
            using (var spooler = new EmailSpooler(deps))
            {
                spooler.Spool();
            }

            // test result
            foreach (var recipient in dto.EmailRecipients)
            {
                email.Received().AddRecipient(recipient.Recipient);
                email.Received().AddBCC(recipient.Recipient);
                email.Received().AddCC(recipient.Recipient);
            }
        }

        [Test]
        public void Spool_WhenOneEmailNotSent_AddsAllAttachmentsWithoutContentIDsAsAttachments()
        {
            // test setup
            var email = CreateSubstituteEmail();
            var func = EmailGeneratorWith(email);
            var dto = EmailBuilder.Create().WithRandomProps().WithRandomRecipient().WithRandomAttachment(2).Build();
            dto.EmailAttachments.First().ContentID = null;
            dto.EmailAttachments.Last().ContentID = null;
            dto.EmailRecipients.First().IsBCC = true;
            dto.EmailRecipients.First().IsCC = true;
            var ctx = SubstituteEmailContextBuilder.Create().WithEmails(dto).Build();
            var deps = IFakeEmailSpoolerDependenciesBuilder.Create()
                        .WithDbContext(ctx)
                        .WithEmailGenerator(func)
                        .Build();
            
            // pre-conditions
            Assert.AreEqual(2, dto.EmailAttachments.Count(a => a.Data.Length > 0 && a.Name.Length > 0));

            // execute test
            using (var spooler = new EmailSpooler(deps))
            {
                spooler.Spool();
            }

            // test result
            foreach (var attachment in dto.EmailAttachments)
            {
                email.Received().AddAttachment(attachment.Name, attachment.Data, attachment.MIMEType);
            }
        }

        [Test]
        public void Spool_WhenOneEmailNotSent_AddsAllAttachmentsWithContentIDsWithContentIds()
        {
            // test setup
            var email = CreateSubstituteEmail();
            var func = EmailGeneratorWith(email);
            var dto = EmailBuilder.Create().WithRandomProps().WithRandomRecipient().WithRandomAttachment(2).Build();
            dto.EmailRecipients.First().IsBCC = true;
            dto.EmailRecipients.First().IsCC = true;
            var ctx = SubstituteEmailContextBuilder.Create().WithEmails(dto).Build();
            var deps = IFakeEmailSpoolerDependenciesBuilder.Create()
                        .WithDbContext(ctx)
                        .WithEmailGenerator(func)
                        .Build();
            
            // pre-conditions
            Assert.AreEqual(2, dto.EmailAttachments.Count(a => a.Data.Length > 0 && a.Name.Length > 0));

            // execute test
            using (var spooler = new EmailSpooler(deps))
            {
                spooler.Spool();
            }

            // test result
            foreach (var attachment in dto.EmailAttachments)
            {
                email.Received().AddAttachment(attachment.Name, attachment.Data, attachment.MIMEType, attachment.ContentID);
            }
        }

        [Test]
        public void Spool_WhenOneEmailNotSent_SendsEmail()
        {
            // test setup
            var email = CreateSubstituteEmail();
            var func = EmailGeneratorWith(email);
            var dto = EmailBuilder.Create().WithRandomProps().WithRandomRecipient().WithRandomAttachment(2).Build();
            dto.EmailRecipients.First().IsBCC = true;
            dto.EmailRecipients.First().IsCC = true;
            var ctx = SubstituteEmailContextBuilder.Create().WithEmails(dto).Build();
            var deps = IFakeEmailSpoolerDependenciesBuilder.Create()
                        .WithDbContext(ctx)
                        .WithEmailGenerator(func)
                        .Build();
            
            // pre-conditions
            Assert.AreEqual(2, dto.EmailAttachments.Count(a => a.Data.Length > 0 && a.Name.Length > 0));

            // execute test
            using (var spooler = new EmailSpooler(deps))
            {
                spooler.Spool();
            }

            // test result
            email.Received().Send();
        }

        [Test]
        public void Spool_WhenOneEmailNotSentButSendTimeInFuture_DoesNotSendEmail()
        {
            // test setup
            var email = CreateSubstituteEmail();
            var func = EmailGeneratorWith(email);
            var dto = EmailBuilder.Create().WithRandomProps().WithRandomRecipient().WithRandomAttachment(2).Build();
            dto.EmailRecipients.First().IsBCC = true;
            dto.EmailRecipients.First().IsCC = true;
            dto.SendAt = DateTime.Now.AddYears(1);
            var ctx = SubstituteEmailContextBuilder.Create().WithEmails(dto).Build();
            var deps = IFakeEmailSpoolerDependenciesBuilder.Create()
                        .WithDbContext(ctx)
                        .WithEmailGenerator(func)
                        .Build();
            
            // pre-conditions
            Assert.AreEqual(2, dto.EmailAttachments.Count(a => a.Data.Length > 0 && a.Name.Length > 0));

            // execute test
            using (var spooler = new EmailSpooler(deps))
            {
                spooler.Spool();
            }

            // test result
            email.DidNotReceive().Send();
        }

        [Test]
        public void Spool_WhenOneEmailNotSentButSendAttemptsExceeded_DoesNotSendMail()
        {
            // test setup
            var email = CreateSubstituteEmail();
            var func = EmailGeneratorWith(email);
            var dto = EmailBuilder.Create().WithRandomProps().WithRandomRecipient().WithRandomAttachment(2).Build();
            var maxAttempts = RandomValueGen.GetRandomInt(3, 6);
            dto.EmailRecipients.First().IsBCC = true;
            dto.EmailRecipients.First().IsCC = true;
            dto.SendAt = DateTime.Now.AddYears(-1);
            dto.SendAttempts = maxAttempts + 1;
            
            var ctx = SubstituteEmailContextBuilder.Create().WithEmails(dto).Build();
            var deps = IFakeEmailSpoolerDependenciesBuilder.Create()
                        .WithDbContext(ctx)
                        .WithEmailGenerator(func)
                        .Build();
            deps.EmailSpoolerConfig.MaxSendAttempts.ReturnsForAnyArgs(ci => maxAttempts);
            // pre-conditions
            Assert.AreEqual(2, dto.EmailAttachments.Count(a => a.Data.Length > 0 && a.Name.Length > 0));

            // execute test
            using (var spooler = new EmailSpooler(deps))
            {
                spooler.Spool();
            }

            // test result
            email.DidNotReceive().Send();
        }

        [Test]
        public void Spool_DoesNotSendDisabledMails()
        {
            // test setup
            var email = CreateSubstituteEmail();
            var func = EmailGeneratorWith(email);
            var dto = EmailBuilder.Create().WithRandomProps().WithRandomRecipient().WithRandomAttachment(2).Build();
            var maxAttempts = RandomValueGen.GetRandomInt(3, 6);
            dto.EmailRecipients.First().IsBCC = true;
            dto.EmailRecipients.First().IsCC = true;
            dto.SendAt = DateTime.Now.AddYears(-1);
            dto.Enabled = false;
            
            var ctx = SubstituteEmailContextBuilder.Create().WithEmails(dto).Build();
            var deps = IFakeEmailSpoolerDependenciesBuilder.Create()
                        .WithDbContext(ctx)
                        .WithEmailGenerator(func)
                        .Build();
            // pre-conditions
            Assert.AreEqual(2, dto.EmailAttachments.Count(a => a.Data.Length > 0 && a.Name.Length > 0));

            // execute test
            using (var spooler = new EmailSpooler(deps))
            {
                spooler.Spool();
            }

            // test result
            email.DidNotReceive().Send();
        }

        [Test]
        public void Spool_WhenOneEmailNotSent_WhenEmailSendsSuccessfully_MarksEmailAsSent()
        {
            // test setup
            var email = CreateSubstituteEmail();
            var func = EmailGeneratorWith(email);
            var dto = EmailBuilder.Create().WithRandomProps().WithRandomRecipient().WithRandomAttachment(2).Build();
            dto.EmailRecipients.First().IsBCC = true;
            dto.EmailRecipients.First().IsCC = true;
            var ctx = SubstituteEmailContextBuilder.Create().WithEmails(dto).Build();
            var deps = IFakeEmailSpoolerDependenciesBuilder.Create()
                        .WithDbContext(ctx)
                        .WithEmailGenerator(func)
                        .Build();
            
            // pre-conditions
            Assert.AreEqual(2, dto.EmailAttachments.Count(a => a.Data.Length > 0 && a.Name.Length > 0));

            // execute test
            ctx.DidNotReceive().SaveChanges();
            using (var spooler = new EmailSpooler(deps))
            {
                spooler.Spool();
            }

            // test result
            Assert.IsTrue(dto.Sent);
            ctx.Received().SaveChanges();

        }

        [Test]
        public void Spool_WhenOneEmailNotSent_WhenEmailFailsToSend_DoesNotThrowAndIncrementsSendAttempts()
        {
            // test setup
            var email = CreateSubstituteEmail();
            var func = EmailGeneratorWith(email);
            var errorMessage = RandomValueGen.GetRandomString();
            email.When(e => e.Send()).Do(ci => { throw new Exception(errorMessage); });
            var dto = EmailBuilder.Create().WithRandomProps().WithRandomRecipient().WithRandomAttachment(2).Build();
            dto.EmailRecipients.First().IsBCC = true;
            dto.EmailRecipients.First().IsCC = true;
            var ctx = SubstituteEmailContextBuilder.Create().WithEmails(dto).Build();
            var deps = IFakeEmailSpoolerDependenciesBuilder.Create()
                        .WithDbContext(ctx)
                        .WithEmailGenerator(func)
                        .Build();
            var backoff = RandomValueGen.GetRandomInt(2, 10);
            deps.EmailSpoolerConfig.BackoffIntervalInMinutes.ReturnsForAnyArgs(ci => backoff);
            // pre-conditions
            Assert.AreEqual(2, dto.EmailAttachments.Count(a => a.Data.Length > 0 && a.Name.Length > 0));
            Assert.AreEqual(0, dto.SendAttempts);

            // execute test
            ctx.DidNotReceive().SaveChanges();
            var beforeRun = DateTime.Now;
            Assert.DoesNotThrow(() =>
                {
                    using (var spooler = new EmailSpooler(deps))
                    {
                        spooler.Spool();
                    }
                });

            // test result
            Assert.IsFalse(dto.Sent);
            Assert.AreEqual(errorMessage, dto.LastError);
            Assert.AreEqual(1, dto.SendAttempts);
            Assert.That(beforeRun.AddMinutes(backoff), Is.LessThanOrEqualTo(dto.SendAt));
            Assert.That(beforeRun.AddMinutes(backoff).AddSeconds(5), Is.GreaterThanOrEqualTo(dto.SendAt));
            ctx.Received().SaveChanges();
        }

        [Test]
        public void Spool_WhenOneEmailNotSent_WhenEmailFailsToSend_BacksOffIncrementallyWithMultiplier()
        {
            // test setup
            var email = CreateSubstituteEmail();
            var func = EmailGeneratorWith(email);
            var errorMessage = RandomValueGen.GetRandomString();
            email.When(e => e.Send()).Do(ci => { throw new Exception(errorMessage); });
            var dto = EmailBuilder.Create().WithRandomProps().WithRandomRecipient().WithRandomAttachment(2).Build();
            dto.EmailRecipients.First().IsBCC = true;
            dto.EmailRecipients.First().IsCC = true;
            dto.SendAttempts = 1;
            var ctx = SubstituteEmailContextBuilder.Create().WithEmails(dto).Build();
            var deps = IFakeEmailSpoolerDependenciesBuilder.Create()
                        .WithDbContext(ctx)
                        .WithEmailGenerator(func)
                        .Build();
            var backoff = RandomValueGen.GetRandomInt(2, 10);
            deps.EmailSpoolerConfig.BackoffIntervalInMinutes.ReturnsForAnyArgs(ci => backoff);
            // pre-conditions
            Assert.AreEqual(2, dto.EmailAttachments.Count(a => a.Data.Length > 0 && a.Name.Length > 0));

            // execute test
            ctx.DidNotReceive().SaveChanges();
            var beforeRun = DateTime.Now;
            Assert.DoesNotThrow(() =>
                {
                    using (var spooler = new EmailSpooler(deps))
                    {
                        spooler.Spool();
                    }
                });

            // test result
            Assert.IsFalse(dto.Sent);
            Assert.AreEqual(errorMessage, dto.LastError);
            Assert.AreEqual(2, dto.SendAttempts);
            Assert.That(beforeRun.AddMinutes(backoff * 4), Is.LessThanOrEqualTo(dto.SendAt));
            Assert.That(beforeRun.AddMinutes(backoff * 4).AddSeconds(5), Is.GreaterThanOrEqualTo(dto.SendAt));
            ctx.Received().SaveChanges();
        }

        [Test]
        public void Spool_ShouldDisposeOfEmailsProvidedByGenerator()
        {
            var email = CreateSubstituteEmail();
            var func = EmailGeneratorWith(email);
            var dto = EmailBuilder.BuildRandomWithRecipient();
            var ctx = SubstituteEmailContextBuilder.Create().WithEmails(dto).Build();
            var deps = IFakeEmailSpoolerDependenciesBuilder.Create()
                        .WithDbContext(ctx)
                        .WithEmailGenerator(func)
                        .Build();
            // pre-conditions
            Assert.IsTrue(dto.EmailRecipients.Any());

            // execute test
            using (var spooler = new EmailSpooler(deps))
            {
                spooler.Spool();
            }

            // test result
            email.Received().Dispose();
        }

        [Test]
        public void Spool_PurgesSentEmailOlderThanConfiguredKeepSentForValue()
        {
            // test setup
            var email = CreateSubstituteEmail();
            var func = EmailGeneratorWith(email);
            var dto = EmailBuilder.BuildRandomWithRecipient();
            var ctx = SubstituteEmailContextBuilder.Create().WithEmails(dto).Build();
            var deps = IFakeEmailSpoolerDependenciesBuilder.Create()
                        .WithDbContext(ctx)
                        .WithEmailGenerator(func)
                        .Build();
            var dayVal = RandomValueGen.GetRandomInt(30, 40);
            deps.EmailSpoolerConfig.PurgeMessageWithAgeInDays.ReturnsForAnyArgs(ci => dayVal);
            dto.Sent = true;
            dto.LastModified = DateTime.Now.AddDays(-dayVal).AddSeconds(-10);
            
            // pre-conditions
            Assert.AreEqual(1, ctx.Emails.Count());


            // execute test
            using (var spooler = new EmailSpooler(deps))
            {
                spooler.Spool();
            }

            // test result
            email.DidNotReceive().Send();
            Assert.AreEqual(0, ctx.Emails.Count());
            ctx.Received().SaveChanges();
        }

        [Test]
        public void Spool_PurgesUnsendableEmailOlderThanConfiguredKeepSentForValue()
        {
            // test setup
            var email = CreateSubstituteEmail();
            var func = EmailGeneratorWith(email);
            var dto = EmailBuilder.BuildRandomWithRecipient();
            var ctx = SubstituteEmailContextBuilder.Create().WithEmails(dto).Build();
            var deps = IFakeEmailSpoolerDependenciesBuilder.Create()
                        .WithDbContext(ctx)
                        .WithEmailGenerator(func)
                        .Build();
            var dayVal = RandomValueGen.GetRandomInt(30, 40);
            deps.EmailSpoolerConfig.PurgeMessageWithAgeInDays.ReturnsForAnyArgs(ci => dayVal);
            dto.Sent = false;
            dto.SendAttempts = deps.EmailSpoolerConfig.MaxSendAttempts;
            dto.LastModified = DateTime.Now.AddDays(-dayVal).AddSeconds(-10);
            
            // pre-conditions
            Assert.AreEqual(1, ctx.Emails.Count());


            // execute test
            using (var spooler = new EmailSpooler(deps))
            {
                spooler.Spool();
            }

            // test result
            email.DidNotReceive().Send();
            Assert.AreEqual(0, ctx.Emails.Count());
            ctx.Received().SaveChanges();
        }
    }
}
