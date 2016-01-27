using PeanutButter.ServiceShell;

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
