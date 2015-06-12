using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeanutButter.ServiceShell;
using ServiceShell;

namespace EmailSpooler.Win32Service
{
    public class EmailSpoolerService: Shell
    {
        public EmailSpoolerService()
        {
            this.ServiceName = "EmailSpooler";
            this.DisplayName = "Email Spooler Service";
            this.Interval = 10;
        }
        protected override void RunOnce()
        {
            var deps = new EmailSpoolerDependencies(this);
            using (var spooler = new EmailSpooler(deps))
            {
                spooler.Spool();
            }
        }
    }
}
