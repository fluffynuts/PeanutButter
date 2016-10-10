using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using PeanutButter.Async.Interfaces;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;

namespace PeanutButter.Async.Tests
{
    [TestFixture]
    public class TestTaskRunner
    {
        private const int TWO_MINUTES = 120000; // allow liberal time for barriers to expire

        [Test]
        public void ITaskRunner_ShouldHaveMethodRunWhichReturnsTheTask()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(ITaskRunner);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var method = sut.GetMethods().FirstOrDefault(mi => mi.Name == "Run" && mi.ReturnType == typeof(Task));

            //---------------Test Result -----------------------
            Assert.IsNotNull(method, "Unable to find suitable Run method on interface");
        }

        [Test]
        public void ITaskRunner_ShouldHaveMethodRunWhichReturnsTheTaskOfTypeT()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(ITaskRunner);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var method = sut.GetMethods().FirstOrDefault(mi => mi.Name == "Run" &&
                                                               mi.ReturnType.IsGenericType &&
                                                               mi.ReturnType.BaseType == typeof(Task));

            //---------------Test Result -----------------------
            Assert.IsNotNull(method, "Unable to find suitable Run method on interface");
        }

        [Test]
        public void Type_ShouldImplement_ITaskRunner()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(TaskRunner);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldImplement<ITaskRunner>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void Run_ShouldRunTheActionInTheBackground()
        {
            //---------------Set up test pack-------------------
            var barrier1 = new Barrier(2);
            var barrier2 = new Barrier(2);
            var sut = Create();

            //---------------Assert Precondition----------------
            var called = false;
            //---------------Execute Test ----------------------
            sut.Run(() =>
            {
                barrier1.SignalAndWait();
                called = true;
                barrier2.SignalAndWait();
            });

            //---------------Test Result -----------------------
            Assert.IsFalse(called);
            barrier1.SignalAndWait(TWO_MINUTES);
            barrier2.SignalAndWait(TWO_MINUTES);
            Assert.IsTrue(called);
        }

        [Test]
        public void Run_ShouldReturnTheTaskThatIsCreated_MakingItAwaitable()
        {
            //---------------Set up test pack-------------------
            var sut = Create();
            var called = false;
            var barrier = new Barrier(2);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var task = sut.Run(() =>
            {
                barrier.SignalAndWait();
                Thread.Sleep(1000);
                called = true;
            });

            //---------------Test Result -----------------------
            barrier.SignalAndWait();
            Assert.IsFalse(called);
            task.Wait();
            Assert.IsTrue(called);
        }

        [Test]
        public void Run_GivenFuncReturningType_ShouldReturnTaskOfThatType()
        {
            //---------------Set up test pack-------------------
            var sut = Create();
            var expected = RandomValueGen.GetRandomString();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = sut.Run(() => expected);

            //---------------Test Result -----------------------
            Assert.IsInstanceOf<Task<string>>(result);
            Assert.AreEqual(expected, result.Result);
        }

        [Test]
        public void RunLong_GivenAction_ShouldRunItLong()
        {
            //---------------Set up test pack-------------------
            var sut = Create();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var task = sut.RunLong(() => { });

            //---------------Test Result -----------------------
            Assert.AreEqual(task.CreationOptions,
                TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach);
            Assert.IsFalse(task.IsCanceled);
        }

        [Test]
        public void RunLong_GivenActionAndCancellationToken_ShouldRunItLong()
        {
            //---------------Set up test pack-------------------
            var sut = Create();
            var cancellationTokenSource = new CancellationTokenSource();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var task = sut.RunLong(() => { Thread.Sleep(1000); }, cancellationTokenSource.Token);
            //---------------Test Result -----------------------
            Assert.AreEqual(task.CreationOptions,
                TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach);
            var propInfo = task.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(pi => pi.Name == "CancellationToken");
            Assert.IsNotNull(propInfo, "Can't delve into the bowels of the task to get its CancellationToken ):");
            var token = propInfo.GetValue(task);
            Assert.AreEqual(cancellationTokenSource.Token, token);
        }

        [Test]
        public void RunLong_GivenActionAndNoCancellationToken_ShouldRunItLongWith_CancellationToken_dot_None()
        {
            //---------------Set up test pack-------------------
            var sut = Create();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var task = sut.RunLong(() => { Thread.Sleep(1000); });
            //---------------Test Result -----------------------
            Assert.AreEqual(task.CreationOptions,
                TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach);
            var propInfo = task.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(pi => pi.Name == "CancellationToken");
            Assert.IsNotNull(propInfo, "Can't delve into the bowels of the task to get its CancellationToken ):");
            var token = propInfo.GetValue(task);
            Assert.AreEqual(CancellationToken.None, token);
        }

        [Test]
        public void CreateNotStartedFor_GivenFunc_ShouldReturnUnstartedTaskWhichCanBeStartedAtTheWhimsyOfOne()
        {
            //---------------Set up test pack-------------------
            var sut = Create();
            var expected = RandomValueGen.GetRandomInt(10, 100);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var task = sut.CreateNotStartedFor(() => expected);

            //---------------Test Result -----------------------
            Assert.IsInstanceOf<Task<int>>(task);
            task.Start();
            var result = task.Result;
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void CreateNotStartedFor_GivenAction_ShouldReturnUnstartedTaskWhichCanBeStartedAtTheWhimsyOfOne()
        {
            //---------------Set up test pack-------------------
            var sut = Create();
            var expected = RandomValueGen.GetRandomInt(10, 100);
            var result = 0;

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var task = sut.CreateNotStartedFor(() => { result = expected; });

            //---------------Test Result -----------------------
            Assert.IsInstanceOf<Task>(task);
            task.Start();
            task.Wait();
            Assert.AreEqual(expected, result);
        }

        //[Test]
        //public void Continue_With_ShouldContinueOriginalTaskWithNewFunc()
        //{
        //    //---------------Set up test pack-------------------
        //    var sut = Create();
        //    var firstNumber = RandomValueGen.GetRandomInt(1, 10);
        //    var secondNumber = RandomValueGen.GetRandomInt(11, 20);

        //    //---------------Assert Precondition----------------
        //    var task1 = sut.Run(() =>
        //    {
        //        Thread.Sleep(250);
        //        return firstNumber;
        //    });
        //    //---------------Execute Test ----------------------

        //    var secondTask = sut.Continue(task1).With(priorTask =>
        //    {
        //        Thread.Sleep(250);
        //        return priorTask.Result + secondNumber;
        //    });

        //    //---------------Test Result -----------------------
        //    Assert.AreEqual(firstNumber + secondNumber, secondTask.Result);
        //}

        [Test]
        public void Continue_With_ShouldContinueOriginalTaskActionWithNewAction()
        {
            //---------------Set up test pack-------------------
            var sut = Create() as TaskRunner;
            var barrier1 = new Barrier(2);
            var barrier2 = new Barrier(2);
            var firstCalled = false;
            var secondCalled = false;

            //---------------Assert Precondition----------------
            var task1 = sut.Run(() =>
            {
                firstCalled = true;
                barrier1.SignalAndWait();
            });
            //---------------Execute Test ----------------------
            sut.Continue(task1).With(t =>
            {
                secondCalled = true;
                barrier2.SignalAndWait();
            });
            barrier1.SignalAndWait();
            barrier2.SignalAndWait();

            //---------------Test Result -----------------------
            // technically, just getting here is a positive result because of the barriers
            Assert.IsTrue(firstCalled);
            Assert.IsTrue(secondCalled);
        }

        [Test]
        public void Continue_With_ShouldContinueNextTaskWithResultFromPriorOneWhereApplicable()
        {
            //---------------Set up test pack-------------------
            var sut = Create() as TaskRunner;
            var barrier1 = new Barrier(2);
            var barrier2 = new Barrier(2);
            var expected = RandomValueGen.GetRandomInt(1, 10);
            var result = -1;
            var task1 = sut.Run(() =>
            {
                barrier1.SignalAndWait();
                return expected;
            });

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.Continue(task1).With(t =>
            {
                result = t.Result;
                barrier2.SignalAndWait();
            });

            //---------------Test Result -----------------------
            barrier1.SignalAndWait();
            barrier2.SignalAndWait();
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Continue_With_ShouldContinueNextTaskWithResultFromPriorOneWhereApplicable_Level2()
        {
            //---------------Set up test pack-------------------
            var sut = Create() as TaskRunner;
            var barrier1 = new Barrier(2);
            var barrier2 = new Barrier(2);
            var expected = RandomValueGen.GetRandomInt(1, 10);
            var task1 = sut.Run(() =>
            {
                barrier1.SignalAndWait();
                return expected;
            });

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var resultTask = sut.Continue(task1).With(t =>
            {
                barrier2.SignalAndWait();
                return t.Result;
            });

            //---------------Test Result -----------------------
            barrier1.SignalAndWait();
            barrier2.SignalAndWait();

            Assert.AreEqual(expected, resultTask.Result);
        }

        [Test]
        public void Continue_With_Fluently()
        {
            //---------------Set up test pack-------------------
            var sut = Create() as TaskRunner;
            var expected = RandomValueGen.GetRandomInt(1, 10);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var resultTask = sut.Run(() => { Thread.Sleep(150); })
                .Using(sut).ContinueWith(t =>
                {
                    Thread.Sleep(100);
                    return expected;
                })
                .Using(sut).ContinueWith(t => t.Result)
                .Using(sut).ContinueWith(t =>
                {
                    Thread.Sleep(250);
                    Console.WriteLine("last task");
                    return t.Result;
                });

            //---------------Test Result -----------------------
            Console.WriteLine("About to wait on result");

            Assert.AreEqual(expected, resultTask.Result);
        }


        private ITaskRunner Create()
        {
            return new TaskRunner();
        }
    }

}
