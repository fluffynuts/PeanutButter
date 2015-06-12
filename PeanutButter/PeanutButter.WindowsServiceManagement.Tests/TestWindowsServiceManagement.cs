using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        [Ignore("Run manually on known services")]
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
        [Ignore("Just interesting")]
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



    }
}
