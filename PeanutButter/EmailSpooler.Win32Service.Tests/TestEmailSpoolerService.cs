using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using PeanutButter.ServiceShell;
using PeanutButter.TestUtils.Generic;

namespace EmailSpooler.Win32Service.Tests
{
    [TestFixture]
    public class TestEmailSpoolerService
    {
        [Test]
        public void Type_ShouldInheritFrom_Shell()
        {
            //---------------Set up test pack-------------------
            var sut = typeof (EmailSpoolerService);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldInheritFrom<Shell>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void RunOnce_ShouldCreateNewSpoolerAndSpoolIt()
        {
            //---------------Set up test pack-------------------
            var spooler1 = Substitute.For<IEmailSpooler>();
            var spooler2 = Substitute.For<IEmailSpooler>();
            var spoolers = new Queue<IEmailSpooler>(new[] {spooler1, spooler2});
            var sut = new EmailSpoolerService_OVERRIDES_SpoolerFactory();
            var dependencyProviders = new List<IEmailSpoolerDependencies>();
            sut.CreateSpooler = deps =>
            {
                dependencyProviders.Add(deps);
                return spoolers.Dequeue();
            };

            //---------------Assert Precondition----------------
            CollectionAssert.IsEmpty(dependencyProviders);
            Assert.AreEqual(2, spoolers.Count);

            //---------------Execute Test ----------------------
            sut.BaseRunOnce();
            Assert.AreEqual(1, spoolers.Count);
            Assert.AreEqual(1, dependencyProviders.Count);
            Assert.AreEqual(sut, (dependencyProviders.First() as EmailSpoolerDependencies)?.Logger);
            spooler1.Received().Spool();
            sut.BaseRunOnce();
            Assert.AreEqual(0, spoolers.Count);
            Assert.AreEqual(2, dependencyProviders.Count);
            Assert.AreEqual(sut, (dependencyProviders.Last() as EmailSpoolerDependencies)?.Logger);
            spooler2.Received().Spool();

            //---------------Test Result -----------------------
        }


        // ReSharper disable once InconsistentNaming
        private class EmailSpoolerService_OVERRIDES_SpoolerFactory: EmailSpoolerService
        {
            public Func<IEmailSpoolerDependencies, IEmailSpooler> CreateSpooler { get; set; }

            protected override IEmailSpooler CreateSpoolerWith(IEmailSpoolerDependencies deps)
            {
                return CreateSpooler(deps);
            }

            public void BaseRunOnce()
            {
                base.RunOnce();
            }
        }

    }
}
