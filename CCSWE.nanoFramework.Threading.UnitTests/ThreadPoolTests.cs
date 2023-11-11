using System;
using nanoFramework.TestFramework;
using System.Threading;
using CCSWE.nanoFramework.Threading.Internal;

namespace CCSWE.nanoFramework.Threading.UnitTests
{
    [TestClass]
    public class ThreadPoolTests
    {
        [TestMethod]
        public void QueueUserWorkItem_returns_false_when_full()
        {
            // Arrange
            var completedEvent = new ManualResetEvent(false);
            var completedThreads = 0;
            var startEvent = new ManualResetEvent(false);
            var startedThreads = 0;

            using var sut = new ThreadPoolInternal(2, 2);

            var expected = sut.WorkItems + sut.Workers;

            for (var i = 0; i < expected; i++)
            {
                sut.QueueUserWorkItem(_ =>
                {
                    Interlocked.Increment(ref startedThreads);
                    startEvent.WaitOne();
                    var completed = Interlocked.Increment(ref completedThreads);

                    if (completed == expected)
                    {
                        completedEvent.Set();
                    }
                });
            }

            // Act
            var workItemQueued = sut.QueueUserWorkItem(_ => { });

            startEvent.Set();

            var completed = completedEvent.WaitOne(10_000, false);

            // Assert
            Assert.IsFalse(workItemQueued, "Work item queued");
            Assert.IsTrue(completed, "Completed");
        }
    }
}
