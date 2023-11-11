using System;
using System.Threading;
using CCSWE.nanoFramework.Threading.Internal;
using nanoFramework.TestFramework;

namespace CCSWE.nanoFramework.Threading.UnitTests
{
    [TestClass]
    public class ConsumerThreadPoolTests
    {
        [TestMethod]
        public void Enqueue_should_throw_ArgumentNullException_if_item_is_null()
        {
            var completed = false;
            using var threadPool = new ThreadPoolInternal(1, 1);
            using var sut = new ConsumerThreadPool(1, _ => { completed = true; }, threadPool);

            Assert.ThrowsException(typeof(ArgumentNullException), () => sut.Enqueue(null!));
            Thread.Sleep(0);

            Assert.IsFalse(completed);
        }

        [TestMethod]
        public void Enqueue_should_throw_ObjectDisposedException_if_disposed()
        {
            var threadPool = new ThreadPoolInternal(1, 1);
            var sut = new ConsumerThreadPool(1, _ => { }, threadPool);
            sut.Dispose();
            threadPool.Dispose();

            Assert.ThrowsException(typeof(ObjectDisposedException), () => sut.Enqueue(new object()));
        }

        [TestMethod]
        public void It_should_execute_callback_for_all_items()
        {
            var completedEvent = new ManualResetEvent(false);
            var expected = 16;
            var processed = 0;
            using var threadPool = new ThreadPoolInternal(expected, expected);

            using var sut = new ConsumerThreadPool(4, item =>
            {
                var current = Interlocked.Increment(ref processed);

                if (current == expected)
                {
                    completedEvent.Set();
                }
            }, threadPool);

            for (var i = 0; i < expected; i++)
            {
                sut.Enqueue(i);
            }

            var completed = completedEvent.WaitOne(10_000, false);

            Assert.AreEqual(expected, processed);
            Assert.IsTrue(completed, "Completed");
        }
    }
}
