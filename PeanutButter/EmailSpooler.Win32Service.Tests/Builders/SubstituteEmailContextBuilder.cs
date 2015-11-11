using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EACH.Tests.Builders;
using EmailSpooler.Win32Service.DB.Entities;
using NSubstitute;
using PeanutButter.RandomGenerators;

namespace EmailSpooler.Win32Service.Tests.Builders
{
    public class SubstituteEmailContextBuilder: BuilderBase<SubstituteEmailContextBuilder, IEmailContext>, IBuilder<IEmailContext>
    {
        public SubstituteEmailContextBuilder()
        {
            this.WithEmails(IDbSetSubstituteBuilder<DB.Entities.Email>.BuildDefault())
                .WithEmailAttachments(IDbSetSubstituteBuilder<DB.Entities.EmailAttachment>.BuildDefault())
                .WithEmailRecipients(IDbSetSubstituteBuilder<EmailRecipient>.BuildDefault());
        }
        public IEmailContext Build()
        {
            var ctx = Substitute.For<IEmailContext>();
            ctx.EmailAttachments = this._EmailAttachments;
            ctx.Emails = this._Emails;
            ctx.EmailRecipients = this._EmailRecipients;
            return ctx;
        }

        private IDbSet<DB.Entities.Email> _Emails;
        public SubstituteEmailContextBuilder WithEmails(IDbSet<DB.Entities.Email> Emails)
        {
            this._Emails = Emails;
            return this;
        }
        public SubstituteEmailContextBuilder WithEmails(params DB.Entities.Email[] emails)
        {
            return AddIfNotThere(emails, this._Emails);
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
                this.AddIfNotThere(item, collection);
            }
            return this;
        }

        private IDbSet<DB.Entities.EmailAttachment> _EmailAttachments;
        public SubstituteEmailContextBuilder WithEmailAttachments(IDbSet<DB.Entities.EmailAttachment> EmailAttachments)
        {
            this._EmailAttachments = EmailAttachments;
            return this;
        }
        public SubstituteEmailContextBuilder WithEmailAttachments(params DB.Entities.EmailAttachment[] attachments)
        {
            return AddIfNotThere(attachments, this._EmailAttachments);
        }

        private IDbSet<EmailRecipient> _EmailRecipients;
        public SubstituteEmailContextBuilder WithEmailRecipients(IDbSet<EmailRecipient> EmailRecipients)
        {
            this._EmailRecipients = EmailRecipients;
            return this;
        }
        public SubstituteEmailContextBuilder WithEmailRecipients(params EmailRecipient[] EmailRecipients)
        {
            return AddIfNotThere(EmailRecipients, this._EmailRecipients);
        }

    }
}
