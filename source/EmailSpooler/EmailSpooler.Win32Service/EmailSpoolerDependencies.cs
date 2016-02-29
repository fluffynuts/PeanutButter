using System;
using EmailSpooler.Win32Service.Entity;
using EmailSpooler.Win32Service.SMTP;
using PeanutButter.ServiceShell;
using Email = EmailSpooler.Win32Service.SMTP.Email;

namespace EmailSpooler.Win32Service
{
    public class EmailSpoolerDependencies: IEmailSpoolerDependencies
    {
        public ISimpleLogger Logger { get; private set; }
        public IEmailContext DbContext { get; private set; }
        public Func<IEmail> EmailGenerator { get; private set; }
        public IEmailSpoolerConfig EmailSpoolerConfig { get; private set; }
        public IEmailConfiguration EmailConfig { get; private set; }

        public EmailSpoolerDependencies(ISimpleLogger logger)
        {
            Logger = logger;
            DbContext = new EmailContext();
            EmailConfig = EmailConfiguration.CreateFromAppConfig();
            EmailSpoolerConfig = new EmailSpoolerConfig(logger);
            EmailGenerator = () => new Email(EmailConfig);
        }
    }
}
