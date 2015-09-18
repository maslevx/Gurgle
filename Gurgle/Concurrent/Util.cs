using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gurgle.Concurrent
{
    internal static class Util
    {
        public static int MaxThreadDefault
        {
            get { return (Environment.ProcessorCount * 2); } //2 per core, need to do testing
        }

        public static int MaxConnectionDefault
        {
            get { return (Environment.ProcessorCount * 12); } //value recommended on msdn
        }

        public static int FrameworkConnections 
        {
            get { return Convert.ToInt32(MaxConnectionDefault * 0.75); }
        }

        //returns input tasks in the order of (future) completion
        internal static Task<Task<T>>[] Interleaved<T>(IEnumerable<Task<T>> tasks)
        {
            List<Task<T>> inputTasks = tasks.ToList();

            var buckets = new TaskCompletionSource<Task<T>>[inputTasks.Count];
            var rtnVal = new Task<Task<T>>[buckets.Length];

            for (int i = 0; i < buckets.Length; i++)
            {
                buckets[i] = new TaskCompletionSource<Task<T>>();
                rtnVal[i] = buckets[i].Task;
            }

            int nextTask = -1;
            Action<Task<T>> contAction = comp =>
            {
                var bucket = buckets[Interlocked.Increment(ref nextTask)];
                bucket.TrySetResult(comp);
            };

            foreach (Task<T> inpuTask in inputTasks)
            {
                inpuTask.ContinueWith(contAction, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
            }

            return rtnVal;
        }
    }
}
