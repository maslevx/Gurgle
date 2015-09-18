using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gurgle.Utils
{
    internal class ThrottledScheduler<T>
    {
        private Task<T>[] m_buffer;
        private readonly object m_lock = new object();
        private readonly ConcurrentQueue<T> m_completed = new ConcurrentQueue<T>();
        private IEnumerator<Task<T>> m_mover;
        private bool m_waiting;

        public int TaskLimit { get; set; }

        public ThrottledScheduler(int limit)
        {
            TaskLimit = limit;
        }

        public ThrottledScheduler()
            : this(50)
        { }


        public IEnumerable<T> Do(IEnumerable<Task<T>> source)
        {
            int limit = TaskLimit;
            m_buffer = new Task<T>[limit];
            m_mover = source.GetEnumerator();

            while (--limit >= 0 && m_mover.MoveNext())
            {
                Task<T> current = m_mover.Current;
                m_buffer[limit] = current;
                current.ContinueWith(OnCompletion);
                TryStart(current);
            }

            while (m_completed.IsEmpty == false || m_buffer.Any(t => t != null))
            {
                if (m_completed.IsEmpty)
                    lock (m_completed)
                    {
                        m_waiting = true;
                        Monitor.Wait(m_completed);
                        m_waiting = false;
                    }
                else
                {
                    T tmp;
                    if (m_completed.TryDequeue(out tmp))
                        yield return tmp;
                }
            }
        }

        private void OnCompletion(Task<T> task)
        {
            m_completed.Enqueue(task.Result);
            if (m_waiting)
                lock (m_completed)
                {
                    Monitor.Pulse(m_completed);
                }

            lock (m_lock)
            {
                int pos = Array.IndexOf(m_buffer, task);
                m_buffer[pos] = null;
                if (m_mover.MoveNext())
                {
                    Task<T> current = m_mover.Current;
                    m_buffer[pos] = current;
                    current.ContinueWith(OnCompletion);
                    TryStart(current);
                }
            }
        }
        
        private static void TryStart(Task obj)
        {
            if (obj.Status == TaskStatus.Created)
                obj.Start();
        }
        
    }
}
