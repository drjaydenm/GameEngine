using System;
using System.Threading;

namespace GameEngine.Core.Threading
{
    public interface IJobQueue
    {
        int JobCount { get; }
        ManualResetEvent JobQueueEmpty { get; }

        void EnqueueJob(Action job);
        void ExecuteSingleJob();
    }
}
