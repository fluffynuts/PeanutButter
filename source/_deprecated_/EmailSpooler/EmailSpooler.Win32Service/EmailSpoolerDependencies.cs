using System;
using EmailSpooler.Win32Service.Entity;
using EmailSpooler.Win32Service.SMTP;
using PeanutButter.ServiceShell;
using Email = EmailSpooler.Win32Service.SMTP.Email;

namespace EmailSpooler.Win32Service
{
    public class EmailSpoolerDependencies: IEmailSpoolerDependencies
    {
        public ISimpleLogger Logger { get; }
        public IEmailContext DbContext { get; }
        public Func<IEmail> EmailGenerator { get; }
        public IEmailSpoolerConfig EmailSpoolerConfig { get; }
        public IEmailConfiguration EmailConfig { get; }

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
