using System.Collections.Concurrent;

namespace GameEngine.Core.Threading
{
    public abstract class JobQueueBase : IJobQueue
    {
        public int JobCount => QueuedJobs.Count;
        public ManualResetEvent JobQueueEmpty { get; }

        protected ConcurrentQueue<Action> QueuedJobs { get; }
        protected AutoResetEvent JobAdded { get; }

        private int jobRunningCount;

        public JobQueueBase()
        {
            QueuedJobs = new ConcurrentQueue<Action>();

            JobQueueEmpty = new ManualResetEvent(true);
            JobAdded = new AutoResetEvent(false);
        }

        public void EnqueueJob(Action job)
        {
            JobQueueEmpty.Reset();

            QueuedJobs.Enqueue(job);

            JobAdded.Set();
        }

        public void ExecuteSingleJob()
        {
            if (QueuedJobs.TryDequeue(out var job))
            {
                Interlocked.Increment(ref jobRunningCount);

                try
                {
                    job();
                }
                catch { }

                Interlocked.Decrement(ref jobRunningCount);

                if (jobRunningCount == 0 && QueuedJobs.Count <= 0)
                    JobQueueEmpty.Set();
            }
        }
    }
}
