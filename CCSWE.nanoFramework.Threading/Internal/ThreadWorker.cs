//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using System.Threading;

namespace CCSWE.nanoFramework.Threading.Internal
{
    // TODO: Allow for worker to be cancelled with CancellationToken?
    internal class ThreadWorker
    {
        private WaitCallback? _callback;
        private readonly object _lock = new();
        private object? _state;
        private Thread? _thread;
        private readonly ThreadPoolInternal _threadPool;
        private readonly AutoResetEvent _workItemPosted = new(false);

        public ThreadWorker(ThreadPoolInternal threadPool)
        {
            _threadPool = threadPool;
        }

        public int Id => _thread is not null ? _thread.ManagedThreadId : -1;

        public bool IsFree => _thread == null || _callback == null; // || _thread.ThreadState == ThreadState.Suspended;

        private void ExecuteWorkItems()
        {
            while (true)
            {
                //Debug.WriteLine($"Thread {Id} started");

                lock (_lock)
                {
                    if (_callback is not null)
                    {
                        _callback(_state);
                        _callback = null;
                        _state = null;
                    }
                }

                _threadPool.ExecutePendingWorkItems();

                // If more work was posted to this worker as a result of call to ExecutePendingWorkItems, continue the work immediately
                if (_callback is not null)
                {
                    continue;
                }

                //Debug.WriteLine($"Thread {Id} exited");

                _workItemPosted.WaitOne();
            }
        }

        public void Post(WaitCallback callback, object? state)
        {
            Ensure.IsNotNull(nameof(callback), callback);

            lock (_lock)
            {
                _callback = callback;
                _state = state;

                Start();
            }
        }

        private void Start()
        {
            if (_thread is null)
            {
                _thread = new Thread(ExecuteWorkItems);
                _thread.Start();
            }
            else
            {
                _workItemPosted.Set();
            }
        }
    }
}
