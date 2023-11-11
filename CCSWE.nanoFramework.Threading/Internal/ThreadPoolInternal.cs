using System.Threading;

namespace CCSWE.nanoFramework.Threading.Internal
{
    /// <summary>
    /// Provides a pool of threads that can be used to execute tasks, post work items, process asynchronous I/O, wait on behalf of other threads, and process timers.
    /// </summary>
    internal class ThreadPoolInternal
    {
        // TODO: Implement idle thread timeout?
        private const int ThreadPoolThreadTimeoutMs = 20 * 1000;

        /// <summary>
        /// Maximum number of workers.
        /// </summary>
        public const int Workers = 64;

        /// <summary>
        /// Maximum number of queued work items. Work is queued when all workers are already working.
        /// This allows for a maximum of (Workers+WorkItems) posted concurrently.
        /// </summary>
        public const int WorkItems = 64;

        // Using fixed arrays for performance
        private WorkItemQueue _pendingWorkItems = new(WorkItems);
        private ThreadWorkerQueue _threadWorkers = new(Workers);

        // ReSharper disable once InconsistentNaming
        private readonly object _lock = new();

        /// <summary>
        ///  Gets the number of work items that are currently queued to be processed.
        /// </summary>
        /// <value>
        /// The number of work items that are currently queued to be processed.
        /// </value>
        public long PendingWorkItemCount => _pendingWorkItems.Count;

        /// <summary>
        /// Gets the number of thread pool threads that currently exist.
        /// </summary>
        /// <value>
        /// The number of thread pool threads that currently exist.
        /// </value>
        public int ThreadCount => _threadWorkers.Count;

        internal void ExecutePendingWorkItems()
        {
            lock (_lock)
            {
                while (PendingWorkItemCount > 0)
                {
                    // Get an available worker
                    var worker = GetOrCreateFreeWorker();
                    if (worker is null)
                    {
                        // All workers are busy. The first to complete will resume this process.
                        break;
                    }

                    WorkItem work = default;

                    if (_pendingWorkItems.Dequeue(ref work))
                    {
                        worker.Post(work.CallBack, work.State);
                    }
                }
            }
        }

        private ThreadWorker? GetFreeWorker()
        {
            // NOTE: Creating outside the loop for efficiency
            // ReSharper disable once TooWideLocalVariableScope
            ThreadWorker worker;

            for (var i = 0; i < ThreadCount; i++)
            {
                worker = _threadWorkers[i];

                if (worker.IsFree)
                {
                    return worker;
                }
            }

            return null;
        }

        private ThreadWorker? GetOrCreateFreeWorker()
        {
            var worker = GetFreeWorker();
            if (worker is not null)
            {
                return worker;
            }

            if (ThreadCount >= Workers)
            {
                return null;
            }

            worker = new ThreadWorker(this);

            _threadWorkers.Enqueue(worker);

            //Debug.WriteLine($"{_threadWorkers.Count} workers started");

            return worker;
        }

        /// <summary>
        /// Queues a method for execution, and specifies an object containing data to be used by the method. The method executes when a thread pool thread becomes available.
        /// </summary>
        /// <param name="callback">A <see cref="WaitCallback"/> representing the method to execute.</param>
        /// <param name="state">An object containing data to be used by the method.</param>
        /// <returns><see langword="true"/> if the method is successfully queued; <see langword="false"/> otherwise.</returns>
        public bool QueueUserWorkItem(WaitCallback callback, object? state = null)
        {
            Ensure.IsNotNull(nameof(callback), callback);

            lock (_lock)
            {
                var worker = GetOrCreateFreeWorker();
                if (worker is null)
                {
                    return _pendingWorkItems.Enqueue(new WorkItem(callback, state));
                }

                worker.Post(callback, state);
                return true;
            }
        }

        // TODO: Implement adjusting thread counts?

        /// <summary>
        /// Sets the number of requests to the thread pool that can be active concurrently. All requests above that number remain queued until thread pool threads become available.
        /// </summary>
        /// <param name="workerThreads">The maximum number of worker threads in the thread pool.</param>
        /// <returns><see langword="true"/> if the change is successful; otherwise, <see langword="false"/>.</returns>
        public bool SetMaxThreads(int workerThreads)
        {
            return false;
        }

        /// <summary>
        /// Sets the minimum number of active worker threads.
        /// </summary>
        /// <param name="workerThreads">The minimum number of active worker threads.</param>
        /// <returns><see langword="true"/> if the change is successful; otherwise, <see langword="false"/>.</returns>
        public bool SetMinThreads(int workerThreads)
        {
            if (workerThreads <= 0)
            {
                return false;
            }

            if (ThreadCount >= Workers)
            {
                return false;
            }

            QueueUserWorkItem(_ =>
            {
                var completionEvent = new ManualResetEvent(false);

                while (ThreadCount < workerThreads)
                {
                    QueueUserWorkItem(_ => { completionEvent.WaitOne(); });
                }

                completionEvent.Set();
            });

            return true;
        }
    }
}
