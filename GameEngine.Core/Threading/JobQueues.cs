using System;
using System.Threading;

namespace GameEngine.Core.Threading
{
    public class JobQueues
    {
        public IJobQueue Background { get; }
        public IJobQueue WhenIdle { get; }

        private CancellationTokenSource cancellationTokenSource;

        public JobQueues()
        {
            cancellationTokenSource = new CancellationTokenSource();

            Background = new ThreadedJobQueue(Environment.ProcessorCount, cancellationTokenSource.Token);
            WhenIdle = new SynchronousJobQueue();
        }

        public void ShutdownQueues()
        {
            cancellationTokenSource.Cancel();
        }
    }
}
