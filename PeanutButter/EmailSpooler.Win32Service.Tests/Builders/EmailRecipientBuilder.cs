using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmailSpooler.Win32Service.DB.Entities;
using PeanutButter.RandomGenerators;

namespace EmailSpooler.Win32Service.Tests.Builders
{
    public class EmailRecipientBuilder: GenericBuilder<EmailRecipientBuilder, EmailRecipient>
    {
        public override EmailRecipientBuilder WithRandomProps()
        {
            return base.WithRandomProps()
                        .WithProp(r => r.Enabled = true);
        }
    }
}
