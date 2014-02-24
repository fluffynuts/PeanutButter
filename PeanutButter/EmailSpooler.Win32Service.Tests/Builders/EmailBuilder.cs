using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeanutButter.RandomGenerators;
using EmailSpooler.Win32Service.Models;

namespace EmailSpooler.Win32Service.Tests.Builders
{
    public class EmailBuilder: GenericBuilder<EmailBuilder,Models.Email>
    {
        public override EmailBuilder WithRandomProps()
        {
            return base.WithRandomProps()
                        .WithProp(e => e.Sent = false)
                        .WithProp(e => e.SendAt = DateTime.Now.AddMinutes(-1))
                        .WithProp(e => e.SendAttempts = 0);
        }
        public EmailBuilder WithRandomCC(int howMany = 1)
        {
                this.WithProp(e => 
                    {
                        for (var i = 0; i < howMany; i++)
                            e.EmailRecipients.Add(EmailRecipientBuilder.Create().WithRandomProps()
                                                                            .WithProp(r =>
                                                                            {
                                                                                r.PrimaryRecipient = false;
                                                                                r.CC = true;
                                                                                r.BCC = false;
                                                                            }).Build());
                    });
            return this;
        }
        public static Models.Email BuildRandomWithRecipient()
        {
            return Create().WithRandomProps().WithRandomRecipient().Build();
        }
        public EmailBuilder WithRandomRecipient(int howMany = 1)
        {
                this.WithProp(e => 
                    {
                        for (var i = 0; i < howMany; i++)
                            e.EmailRecipients.Add(EmailRecipientBuilder.Create().WithRandomProps()
                                .WithProp(r =>
                                {
                                    r.PrimaryRecipient = true;
                                    r.CC = false;
                                    r.BCC = false;
                                }).Build());
                });
            return this;
        }
        public EmailBuilder WithRandomBCC(int howMany = 1)
        {
                this.WithProp(e =>
                    {
                        for (var i = 0; i < howMany; i++)
                            e.EmailRecipients.Add(EmailRecipientBuilder.Create().WithRandomProps()
                                                                            .WithProp(r =>
                                                                            {
                                                                                r.PrimaryRecipient = false;
                                                                                r.CC = false;
                                                                                r.BCC = true;
                                                                            }).Build());
                    });
            return this;
        }
        public EmailBuilder WithRandomAttachment(int howMany = 1)
        {
            this.WithProp(e =>
                {
                    for (var i = 0; i < howMany; i++)
                        e.EmailAttachments.Add(EmailAttachmentBuilder.BuildRandom());
                });
            return this;
        }
    }
}
