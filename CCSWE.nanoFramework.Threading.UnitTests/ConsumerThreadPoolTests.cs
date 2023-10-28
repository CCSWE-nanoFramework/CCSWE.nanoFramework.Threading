using System;
using System.Threading;
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
            var sut = new ConsumerThreadPool(1, _ => { completed = true; });

            Assert.ThrowsException(typeof(ArgumentNullException), () => sut.Enqueue(null));
            Thread.Sleep(0);

            sut.Dispose();

            Assert.IsFalse(completed);
        }

        [TestMethod]
        public void Enqueue_should_throw_ObjectDisposedException_if_disposed()
        {
            var sut = new ConsumerThreadPool(1, _ => { });
            sut.Dispose();

            Assert.ThrowsException(typeof(ObjectDisposedException), () => sut.Enqueue(new object()));
        }

        [TestMethod]
        public void It_should_execute_callback_for_all_items()
        {
            var complete = new ManualResetEvent(false);
            var expected = 64;
            var processed = 0;

            var sut = new ConsumerThreadPool(4, item =>
            {
                var current = Interlocked.Increment(ref processed);

                if (current == expected)
                {
                    complete.Set();
                }
            });

            for (var i = 0; i < expected; i++)
            {
                sut.Enqueue(i);
            }

            var result = complete.WaitOne(10_000, false);

            sut.Dispose();

            Assert.AreEqual(expected, processed);
            Assert.IsTrue(result);
        }

    }
}
