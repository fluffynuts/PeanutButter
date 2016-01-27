using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EACH.Tests.Builders;
using EmailSpooler.Win32Service.Entity;
using NSubstitute;
using PeanutButter.RandomGenerators;

namespace EmailSpooler.Win32Service.Tests.Builders
{
    public class SubstituteEmailContextBuilder: BuilderBase<SubstituteEmailContextBuilder, IEmailContext>, IBuilder<IEmailContext>
    {
        public SubstituteEmailContextBuilder()
        {
            WithEmails(IDbSetSubstituteBuilder<Email>.BuildDefault())
                .WithEmailAttachments(IDbSetSubstituteBuilder<EmailAttachment>.BuildDefault())
                .WithEmailRecipients(IDbSetSubstituteBuilder<EmailRecipient>.BuildDefault());
        }
        public IEmailContext Build()
        {
            var ctx = Substitute.For<IEmailContext>();
            ctx.EmailAttachments = _EmailAttachments;
            ctx.Emails = _Emails;
            ctx.EmailRecipients = _EmailRecipients;
            return ctx;
        }

        private IDbSet<Email> _Emails;
        public SubstituteEmailContextBuilder WithEmails(IDbSet<Email> Emails)
        {
            _Emails = Emails;
            return this;
        }
        public SubstituteEmailContextBuilder WithEmails(params Email[] emails)
        {
            return AddIfNotThere(emails, _Emails);
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

        private IDbSet<EmailAttachment> _EmailAttachments;
        public SubstituteEmailContextBuilder WithEmailAttachments(IDbSet<EmailAttachment> EmailAttachments)
        {
            _EmailAttachments = EmailAttachments;
            return this;
        }
        public SubstituteEmailContextBuilder WithEmailAttachments(params EmailAttachment[] attachments)
        {
            return AddIfNotThere(attachments, _EmailAttachments);
        }

        private IDbSet<EmailRecipient> _EmailRecipients;
        public SubstituteEmailContextBuilder WithEmailRecipients(IDbSet<EmailRecipient> EmailRecipients)
        {
            _EmailRecipients = EmailRecipients;
            return this;
        }
        public SubstituteEmailContextBuilder WithEmailRecipients(params EmailRecipient[] EmailRecipients)
        {
            return AddIfNotThere(EmailRecipients, _EmailRecipients);
        }

    }
}
