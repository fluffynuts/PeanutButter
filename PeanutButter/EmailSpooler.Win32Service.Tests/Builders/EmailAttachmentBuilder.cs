using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeanutButter.RandomGenerators;

namespace EmailSpooler.Win32Service.Tests.Builders
{
    public class EmailAttachmentBuilder: GenericBuilder<EmailAttachmentBuilder,Models.EmailAttachment>
    {
        public override EmailAttachmentBuilder WithRandomProps()
        {
            return base.WithRandomProps()
                .WithProp(a => a.Name = RandomValueGen.GetRandomFileName())
                .WithProp(a => a.Enabled = true);
        }
    }
}
