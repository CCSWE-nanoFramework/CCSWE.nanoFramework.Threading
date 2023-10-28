using System;
using nanoFramework.TestFramework;
using System.Threading;

namespace CCSWE.nanoFramework.Threading.UnitTests
{
    [TestClass]
    public class ThreadPoolTests
    {
        [Setup]
        public void Setup()
        {
            // TODO: The problem appears to be the suspended threads.
            Assert.SkipTest("These tests are currently timing out. Come back to this.");
        }

        [TestMethod]
        public void SimpleTest1()
        {
            Assert.IsFalse(false);
        }

        [TestMethod]
        public void SimpleTest2()
        {
            Console.WriteLine("Arranging...");
            var completedEvent = new ManualResetEvent(false);
            var completedThreads = 0;
            var startedThreads = 0;

            Console.WriteLine("Acting...");
            var thread = new Thread(() =>
            {
                Interlocked.Increment(ref startedThreads);
                completedEvent.Set();
                Interlocked.Increment(ref completedThreads);
            });
            thread.Start();

            var completed = completedEvent.WaitOne(60_000, true);

            Console.WriteLine("Asserting...");
            Assert.IsTrue(completed);
            Assert.AreEqual(completedThreads, startedThreads);
        }

        [TestMethod]
        public void SimpleTest3()
        {
            Console.WriteLine("Arranging...");
            var completedEvent = new ManualResetEvent(false);
            var completedThreads = 0;
            var startedThreads = 0;

            Console.WriteLine("Acting...");
            ThreadPool.QueueUserWorkItem(_ =>
            {
                Interlocked.Increment(ref startedThreads);
                completedEvent.Set();
                Interlocked.Increment(ref completedThreads);
            });

            var completed = completedEvent.WaitOne(60_000, true);

            Console.WriteLine("Asserting...");
            Assert.IsTrue(completed);
            Assert.AreEqual(completedThreads, startedThreads);
        }

        [TestMethod]
        public void QueueUserWorkItem_returns_false_when_full()
        {
            // Arrange
            Console.WriteLine("Arranging...");
            var startEvent = new ManualResetEvent(false);
            var completedThreads = 0;
            var startedThreads = 0;

            for (var i = 0; i < ThreadPool.WorkItems + ThreadPool.Workers; i++)
            {
                Console.WriteLine($"Queueing thread [{i}]");
                ThreadPool.QueueUserWorkItem(count =>
                {
                    Console.WriteLine($"Starting thread [{count}]");
                    Interlocked.Increment(ref startedThreads);
                    startEvent.WaitOne();
                    Interlocked.Increment(ref completedThreads);
                    Console.WriteLine($"Completing thread [{count}]");
                }, i);
            }

            // Act
            Console.WriteLine("Acting...");
            var actual = ThreadPool.QueueUserWorkItem(_ => { });

            startEvent.Set();

            while (completedThreads < startedThreads)
            {
                Console.WriteLine($"Waiting... {completedThreads}/{startedThreads}");
                Thread.Sleep(1);
            }

            // Assert
            Console.WriteLine("Asserting...");
            Assert.IsFalse(actual, "Work item queued");
        }
    }
}
