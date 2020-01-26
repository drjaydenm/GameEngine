using System;
using System.Threading;

namespace GameEngine.Core.Threading
{
    public class JobQueues
    {
        public ThreadedJobQueue Background { get; }
        public SynchronousJobQueue WhenIdle { get; }

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
