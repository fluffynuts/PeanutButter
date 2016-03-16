using System.Linq;
using NUnit.Framework;
using PeanutButter.ServiceShell;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace EmailSpooler.Win32Service.Tests
{
    [TestFixture]
    public class TestProgram
    {
        [Test]
        public void Main_ShouldInvoke_Shell_RunMain_WithEmailSpoolerServiceType_AndAllProvidedArgs()
        {
            //---------------Set up test pack-------------------
            var args = GetRandomCollection<string>(2,5).ToArray();
            Shell.StartTesting();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = Program.Main(args);

            //---------------Test Result -----------------------
            Assert.AreEqual(-1, result);
            Shell.ShouldHaveRunMainFor<EmailSpoolerService>(args);
        }

    }
}