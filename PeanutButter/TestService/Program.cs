using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PeanutButter.ServiceShell;

namespace TestService
{
    class Program
    {
        static void Main(string[] args)
        {
        }
    }

    public class TestServiceShell: Shell
    {
        public TestServiceShell()
        {
            DisplayName = "Test Service";
            ServiceName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().FullName);
            Version.Major = 1;
        }

        protected override void RunOnce()
        {
            Log("Running once");
        }
    }
}
