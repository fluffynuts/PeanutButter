using EmailSpooler.Win32Service.Entity;
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
