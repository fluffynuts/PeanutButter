using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.Win32ServiceControl;

namespace PeanutButter.WindowsServiceManagement.Tests
{
    [TestFixture]
    public class TestWindowsServiceManagement
    {
        [Test]
        [Explicit("Run manually on known services")]
        public void ACCEPT_WhenTestingStartupStateOfKnownServices_ShouldReturnExpectedState()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var svc1 = new WindowsServiceUtil("mssqlserver");
            Assert.AreEqual(ServiceStartupTypes.Automatic, svc1.StartupType);
            var svc2 = new WindowsServiceUtil("gupdatem");
            Assert.AreEqual(ServiceStartupTypes.Manual, svc2.StartupType);

            //---------------Test Result -----------------------
        }

        [Test]
        [STAThread]
        [Explicit("Just interesting")]
        public void Method_WhenCondition_ShouldExpectedBehaviour()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ver = (new WebBrowser()).Version.Major;
            //---------------Test Result -----------------------
            Console.WriteLine(ver);
        }

        [Test]
        public void ShouldNotBorkWhenNoService()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------
            var svc = new WindowsServiceUtil(RandomValueGen.GetRandomString(20, 30));
            //---------------Execute Test ----------------------
            Assert.IsFalse(svc.IsInstalled);

            //---------------Test Result -----------------------
        }

        [Test]
        [Explicit("manually testing some stuff")]
        public void InstallThing()
        {
            //---------------Set up test pack-------------------
            var path = Path.Combine(Path.GetDirectoryName(GetType().Assembly.CodeBase), "test-service.exe");
            var localPath = new Uri(path).LocalPath;

            //---------------Assert Precondition----------------
            Assert.IsTrue(File.Exists(localPath));

            //---------------Execute Test ----------------------
            var proc = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = localPath,
                    Arguments = "-i"
                }
            };
            Assert.IsTrue(proc.Start());
            proc.WaitForExit();

            //---------------Test Result -----------------------
        }


        [Test]
        [Explicit("manually testing something")]
        public void ReinstallThing()
        {
            //---------------Set up test pack-------------------
            var util = new WindowsServiceUtil("test-service");

            //---------------Assert Precondition----------------
            Assert.IsTrue(util.IsInstalled);

            //---------------Execute Test ----------------------
            util.Uninstall(true);

            //---------------Test Result -----------------------
        }

        [Test]
        [Explicit("Run manually, may be system-specific")]
        public void ServiceExe_ShouldReturnPathTo()
        {
            //---------------Set up test pack-------------------
            var util = new WindowsServiceUtil("Themes");

            //---------------Assert Precondition----------------
            Assert.IsTrue(util.IsInstalled);

            //---------------Execute Test ----------------------
            var path = util.ServiceExe;

            //---------------Test Result -----------------------
            Assert.IsNotNull(path);
            Assert.AreEqual("c:\\windows\\system32\\svchost.exe -k netsvcs", path.ToLower());
        }
    }
}
