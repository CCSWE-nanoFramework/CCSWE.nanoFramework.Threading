using System;
using System.Diagnostics;
using nanoFramework.Benchmark;
using System.Threading;
using GC = nanoFramework.Runtime.Native.GC;

namespace CCSWE.nanoFramework.Threading.Benchmarks
{
    public class Program
    {
        public static void Main()
        {
            //BenchmarkRunner.Run(typeof(Program).Assembly);
            ThreadPool.SetMinThreads(ThreadPool.Workers / 4);
            ThreadPool.QueueUserWorkItem(QueueWorkItems);
            Thread.Sleep(Timeout.Infinite);
        }

        private static int _completed = 0;
        private static int _queued = 0;
        private static int _running = 0;

        private static readonly Random _random = new();

        public static void QueueWorkItems(object state)
        {
            var startTime = DateTime.UtcNow;

            while (true)
            {
                var workItemState = new WorkItemState();
                var queued = ThreadPool.QueueUserWorkItem(WorkItemCallback, workItemState);

                if (queued)
                {
                    Interlocked.Increment(ref _queued);
                }

                Thread.Sleep(queued ? 1 : 1000);
                Debug.WriteLine($"Work {_completed}/{_queued} ({_running}/{_queued - _completed - _running}) - Threads {ThreadPool.ThreadCount} - {DateTime.UtcNow - startTime}");

                GC.Run(true);
            }
        }

        private static void WorkItemCallback(object state)
        {
            Interlocked.Increment(ref _running);

            var workItemState = (WorkItemState)state;
            Thread.Sleep(workItemState.MillisecondsTimeout); // Simulate work

            Debug.WriteLine($"Executed {workItemState.Id}");
            Interlocked.Increment(ref _completed);
            Interlocked.Decrement(ref _running);
        }

        internal class WorkItemState
        {
            public Guid Id { get; } = Guid.NewGuid();
            public int MillisecondsTimeout { get; } = _random.Next(10_000);
        }
    }
}
