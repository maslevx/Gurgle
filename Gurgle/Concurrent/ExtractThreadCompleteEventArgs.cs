using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gurgle.Concurrent
{
    public class ExtractThreadCompleteEventArgs<T>
    {
        private T[] _records;
        public T[] Records
        { get { return _records; } }

        public ExtractThreadCompleteEventArgs(T[] recs)
        {
            _records = recs;
        }
    }

}
