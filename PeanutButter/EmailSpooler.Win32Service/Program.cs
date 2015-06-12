using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeanutButter.ServiceShell;
using ServiceShell;

namespace EmailSpooler.Win32Service
{
    public class Program
    {
        public static int Main(string[] args)
        {
            return Shell.RunMain<EmailSpoolerService>(args);
        }
    }
}
