using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestSingleDebuggerBlock
    {
        [Test]
        public void ShouldOnlyAllowOneRunAtATime()
        {
            // Arrange
            if (!Debugger.IsAttached)
            {
                Assert.Ignore("No debugger attached");
                return;
            }

            var count = 0;
            var fails = 0;
            var threadCount = GetRandomInt(20, 40);
            var threads = new List<Thread>();
            // Act
            for (var i = 0; i < threadCount; i++)
            {
                threads.Add(StartThread(Run));
            }
            
            threads.JoinAll();

            // Assert
            Expect(fails)
                .To.Equal(0);

            Thread StartThread(ThreadStart toRun)
            {
                var result = new Thread(toRun);
                result.Start();
                return result;
            }

            void Run()
            {
                using var _ = new SingleDebuggerBlock("test");
                Thread.Sleep(GetRandomInt(20, 50));
                if (++count > 1)
                {
                    fails++;
                    throw new Exception("running concurrently");
                }

                Thread.Sleep(GetRandomInt(20, 50));
                count--;
            }
        }
    }
}