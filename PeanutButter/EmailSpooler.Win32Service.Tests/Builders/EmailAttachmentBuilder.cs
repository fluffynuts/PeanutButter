using EmailSpooler.Win32Service.Entity;
using PeanutButter.RandomGenerators;

namespace EmailSpooler.Win32Service.Tests.Builders
{
    public class EmailAttachmentBuilder: GenericBuilder<EmailAttachmentBuilder,EmailAttachment>
    {
        public override EmailAttachmentBuilder WithRandomProps()
        {
            return base.WithRandomProps()
                .WithProp(a => a.Name = RandomValueGen.GetRandomFileName())
                .WithProp(a => a.Enabled = true);
        }
    }
}
