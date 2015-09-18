using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Gurgle.Concurrent
{
    class ConcurrentWorkManager<T>
    {
        private Thread[] workerThreads;
        private ConcurrentQueue<T> queue = new ConcurrentQueue<T>();
        private bool shutdown = false;
        private int threadsWaiting = 0;

        private Func<Action<T>> workerFactory;
        private int concurrentLevel;

        public ConcurrentWorkManager(Func<Action<T>> workerFactory)
            : this(Environment.ProcessorCount, workerFactory)
        { }

        public ConcurrentWorkManager(int workerCount, Func<Action<T>> workerFactory)
        {
            concurrentLevel = workerCount;
            workerFactory = workerFactory;

            
        }

        public void AddToQueue(T item)
        {
            EnsureThreads();
            queue.Enqueue(item);
            if (threadsWaiting > 0)
                lock (queue)
                {
                    Monitor.Pulse(queue);
                }
        }

        public void AddToQueue(IEnumerable<T> items)
        {
            EnsureThreads();
            foreach (T item in items)
            {
                queue.Enqueue(item);
            }
            if (threadsWaiting > 0)
                lock (queue)
                {
                    Monitor.Pulse(queue);
                }
        }

        public void WaitForCompletion()
        {
            shutdown = true;

            lock (queue)
            {
                Monitor.PulseAll(queue);
            }

            foreach (Thread t in workerThreads)
                t.Join();

            workerThreads = null;
        }


        private void EnsureThreads()
        {
            if (workerThreads == null)
            {
                workerThreads = new Thread[concurrentLevel];
                for (int i = 0; i < workerThreads.Length; i++)
                {
                    workerThreads[i] = new Thread(() => DispatchLoop(workerFactory()));
                    workerThreads[i].Start();
                }
            }
            //else if (workerThreads.Any(t => !t.IsAlive))
            //{
            //    foreach (Thread workerThread in workerThreads)
            //    {
            //        if (!workerThread.IsAlive)
            //            workerThread.Start();
            //    }
            //}
        }

        private void DispatchLoop(Action<T> workerAction)
        {
            T work;
            while (true)
            {
                if (queue.Count != 0)
                {
                    if (queue.TryDequeue(out work))
                        workerAction(work);
                }
                else //wait for more queued items
                {
                    if (shutdown) //kill signal sent
                        return;

                    lock (queue)
                    {
                        threadsWaiting++;
                        try { Monitor.Wait(queue); }
                        finally { threadsWaiting--; }
                    }
                }
            }
        }
    }
}
