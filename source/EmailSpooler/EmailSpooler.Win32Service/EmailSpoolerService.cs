using PeanutButter.ServiceShell;

namespace EmailSpooler.Win32Service
{
    public class EmailSpoolerService: Shell
    {
        public EmailSpoolerService()
        {
            ServiceName = "EmailSpooler";
            DisplayName = "Email Spooler Service";
            Interval = 10;
        }

        protected override void RunOnce()
        {
            var deps = new EmailSpoolerDependencies(this);
            using (var spooler = CreateSpoolerWith(deps))
            {
                spooler.Spool();
            }
        }

        protected virtual IEmailSpooler CreateSpoolerWith(IEmailSpoolerDependencies deps)
        {
            return new EmailSpooler(deps);
        }
    }
}
