using System;
using System.Threading;
using BepuUtilities;
using BepuUtilities.Memory;

namespace GameEngine.Core.Physics.BepuPhysics
{
    public class DefaultThreadDispatcher : IThreadDispatcher, IDisposable
    {
        struct Worker
        {
            public Thread Thread;
            public AutoResetEvent Signal;
        }

        public int ThreadCount => threadCount;

        private int threadCount;
        private Worker[] workers;
        private AutoResetEvent finished;
        private BufferPool[] bufferPools;

        public DefaultThreadDispatcher(int threadCount)
        {
            this.threadCount = threadCount;
            workers = new Worker[threadCount - 1];
            for (int i = 0; i < workers.Length; ++i)
            {
                workers[i] = new Worker { Thread = new Thread(WorkerLoop), Signal = new AutoResetEvent(false) };
                workers[i].Thread.IsBackground = true;
                workers[i].Thread.Start(workers[i].Signal);
            }
            finished = new AutoResetEvent(false);
            bufferPools = new BufferPool[threadCount];
            for (int i = 0; i < bufferPools.Length; ++i)
            {
                bufferPools[i] = new BufferPool();
            }
        }

        void DispatchThread(int workerIndex)
        {
            if (this.workerBody == null)
                throw new Exception("workerBody is null");

            workerBody(workerIndex);

            if (Interlocked.Increment(ref completedWorkerCounter) == threadCount)
            {
                finished.Set();
            }
        }

        volatile Action<int> workerBody;
        int workerIndex;
        int completedWorkerCounter;

        void WorkerLoop(object untypedSignal)
        {
            var signal = (AutoResetEvent)untypedSignal;
            while (true)
            {
                signal.WaitOne();
                if (disposed)
                    return;
                DispatchThread(Interlocked.Increment(ref workerIndex) - 1);
            }
        }

        void SignalThreads()
        {
            for (int i = 0; i < workers.Length; ++i)
            {
                workers[i].Signal.Set();
            }
        }

        public void DispatchWorkers(Action<int> workerBody)
        {
            if (this.workerBody != null)
                throw new Exception("workerBody is not null");

            workerIndex = 1;
            completedWorkerCounter = 0;
            this.workerBody = workerBody;
            SignalThreads();
            
            DispatchThread(0);
            finished.WaitOne();
            this.workerBody = null;
        }

        volatile bool disposed;
        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                SignalThreads();
                for (int i = 0; i < bufferPools.Length; ++i)
                {
                    bufferPools[i].Clear();
                }
                foreach (var worker in workers)
                {
                    worker.Thread.Join();
                    worker.Signal.Dispose();
                }
            }
        }

        public BufferPool GetThreadMemoryPool(int workerIndex)
        {
            return bufferPools[workerIndex];
        }
    }
}
