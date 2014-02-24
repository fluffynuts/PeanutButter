using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeanutButter.RandomGenerators;

namespace EmailSpooler.Win32Service.Tests.Builders
{
    public class EmailRecipientBuilder: GenericBuilder<EmailRecipientBuilder, Models.EmailRecipient>
    {
        public override EmailRecipientBuilder WithRandomProps()
        {
            return base.WithRandomProps()
                        .WithProp(r => r.Enabled = true);
        }
    }
}
