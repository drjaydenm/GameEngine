using System;
using System.Threading;

namespace GameEngine.Core.Threading
{
    public class ThreadedJobQueue : JobQueueBase
    {
        private CancellationToken cancellationToken;
        private Thread[] workerThreads;

        public ThreadedJobQueue(int numberOfThreads, CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;

            workerThreads = new Thread[numberOfThreads];
            for (var i = 0; i < workerThreads.Length; i++)
            {
                var thread = new Thread(JobLoop);
                thread.Name = nameof(ThreadedJobQueue) + i;
                thread.Start();

                workerThreads[i] = thread;
            }
        }

        private void JobLoop()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (JobAdded.WaitOne(TimeSpan.FromMilliseconds(500)))
                {
                    while (QueuedJobs.Count > 0)
                    {
                        ExecuteSingleJob();
                    }
                }
            }
        }
    }
}
