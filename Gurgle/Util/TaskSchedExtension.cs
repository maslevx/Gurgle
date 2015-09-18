using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gurgle.Utils
{
    //todo: docs
    public static class TaskSchedExtension
    {
        public static IEnumerable<TResult> DoTask<TSource, TResult>(this IEnumerable<TSource> source,
            Func<TSource, Task<TResult>> action, int limit)
        {
            return source.Select(item => action(item)).RunTasks(limit);
        }

        public static IEnumerable<TResult> DoTask<TSource, TResult>(this IEnumerable<TSource> source,
            Func<TSource, TResult> action, int limit)
        {
            return source.Select(item => new Task<TResult>(() => action(item))).RunTasks(limit);
        }

        public static IEnumerable<T> RunTasks<T>(this IEnumerable<Task<T>> source, int limit)
        {
            ThrottledScheduler<T> ts = new ThrottledScheduler<T>(limit);
            return ts.Do(source);
        }
    }
}
