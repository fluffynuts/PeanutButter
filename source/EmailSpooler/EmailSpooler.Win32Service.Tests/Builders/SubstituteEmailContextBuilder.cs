using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EmailSpooler.Win32Service.Entity;
using NSubstitute;
using PeanutButter.RandomGenerators;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMethodReturnValue.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global

namespace EmailSpooler.Win32Service.Tests.Builders
{
    public class SubstituteEmailContextBuilder: BuilderBase<SubstituteEmailContextBuilder, IEmailContext>, IBuilder<IEmailContext>
    {
        public SubstituteEmailContextBuilder()
        {
            WithEmails(DbSetSubstituteBuilder<Email>.BuildDefault())
                .WithEmailAttachments(DbSetSubstituteBuilder<EmailAttachment>.BuildDefault())
                .WithEmailRecipients(DbSetSubstituteBuilder<EmailRecipient>.BuildDefault());
        }
        public IEmailContext Build()
        {
            var ctx = Substitute.For<IEmailContext>();
            ctx.EmailAttachments = _emailAttachments;
            ctx.Emails = _emails;
            ctx.EmailRecipients = _emailRecipients;
            return ctx;
        }

        private IDbSet<Email> _emails;
        public SubstituteEmailContextBuilder WithEmails(IDbSet<Email> emails)
        {
            _emails = emails;
            return this;
        }
        public SubstituteEmailContextBuilder WithEmails(params Email[] emails)
        {
            return AddIfNotThere(emails, _emails);
        }

        private SubstituteEmailContextBuilder AddIfNotThere<T>(T dto, IDbSet<T> collection) where T: class
        {
            if (dto == null) return this;
            if (!collection.Any(existing => existing == dto))
            {
                collection.Add(dto);
            }
            return this;
        }

        private SubstituteEmailContextBuilder AddIfNotThere<T>(IEnumerable<T> src, IDbSet<T> collection) where T: class
        {
            if (src == null) return this;
            foreach (var item in src)
            {
                AddIfNotThere(item, collection);
            }
            return this;
        }

        private IDbSet<EmailAttachment> _emailAttachments;
        public SubstituteEmailContextBuilder WithEmailAttachments(IDbSet<EmailAttachment> emailAttachments)
        {
            _emailAttachments = emailAttachments;
            return this;
        }
        public SubstituteEmailContextBuilder WithEmailAttachments(params EmailAttachment[] attachments)
        {
            return AddIfNotThere(attachments, _emailAttachments);
        }

        private IDbSet<EmailRecipient> _emailRecipients;
        public SubstituteEmailContextBuilder WithEmailRecipients(IDbSet<EmailRecipient> emailRecipients)
        {
            _emailRecipients = emailRecipients;
            return this;
        }
        public SubstituteEmailContextBuilder WithEmailRecipients(params EmailRecipient[] emailRecipients)
        {
            return AddIfNotThere(emailRecipients, _emailRecipients);
        }

    }
}
