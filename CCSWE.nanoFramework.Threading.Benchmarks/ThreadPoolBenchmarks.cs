using System;
using System.Diagnostics;
using System.Threading;
using nanoFramework.Benchmark;
using nanoFramework.Benchmark.Attributes;

namespace CCSWE.nanoFramework.Threading.Benchmarks
{
    [IterationCount(5)]
    public class ThreadPoolBenchmarks: BenchmarkBase
    {
        [Setup]
        public void Setup()
        {
            // Spin the threads up
            RunIterations(() => ThreadPool.QueueUserWorkItem(_ => { }), ThreadPool.Workers);
        }

        [Benchmark]
        public void QueueUserWorkItem()
        {
            RunIterations(() => ThreadPool.QueueUserWorkItem(_ => { Thread.Sleep(0); }), ThreadPool.Workers + ThreadPool.WorkItems);
        }
    }
}
