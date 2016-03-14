using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.ServiceShell.Tests
{
    public class SomeService: Shell
    {
        public SomeService()
        {
            ServiceName = "SomeService";
            DisplayName = "SomeService";
        }
    }

    public class AnotherService: Shell
    {
    }

    public class Program
    {
        public static int Main(string[] args)
        {
            return Shell.RunMain<SomeService>(args);
        }
    }

    [TestFixture]
    public class TestShell
    {
        [Test]
        public void ShouldHaveRunMainFor_WhenRunMainCalledWithMatchingTypeAndParameters_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            Shell.StartTesting();
            var args = GetRandomCollection<string>(2,3).ToArray();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Program.Main(args);
            Assert.DoesNotThrow(() => Shell.ShouldHaveRunMainFor<SomeService>(args));
            //---------------Test Result -----------------------
            
        }

        [Test]
        public void ShouldHaveRunMainFor_WhenRunMainCalledWithMisMatchingTypeAndMatchingParameters_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            Shell.StartTesting();
            var args = GetRandomCollection<string>(2,3).ToArray();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Program.Main(args);
            Assert.Throws<ShellTestFailureException>(() => Shell.ShouldHaveRunMainFor<AnotherService>(args));
            //---------------Test Result -----------------------
            
        }

        [Test]
        public void ShouldHaveRunMainFor_WhenRunMainCalledWithMatchingTypeAndDifferentParameters_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            Shell.StartTesting();
            var args = GetRandomCollection<string>(2,3).ToArray();
            var otherArgs = GetRandomCollection<string>(args.Length, args.Length).ToArray();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Program.Main(args);
            Assert.Throws<ShellTestFailureException>(() => Shell.ShouldHaveRunMainFor<SomeService>(otherArgs));
            //---------------Test Result -----------------------
            
        }

        [Test]
        public void ShouldHaveRunMainFor_WhenNotInTestMode_ShouldThrowException()
        {
            //---------------Set up test pack-------------------
            var args = new[] { "-v" };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Program.Main(args);

            //---------------Test Result -----------------------
            Assert.Throws<ShellTestFailureException>(() => Shell.ShouldHaveRunMainFor<SomeService>(args));
        }


    }
}
