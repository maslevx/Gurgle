using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gurgle
{
    public class AfterInsertRecordEventArgs<TSource,TRecord> : AfterPostEventBase
    {
        public TRecord Record { get; private set; }
        public TSource DataObject { get; private set; }

        public AfterInsertRecordEventArgs(TRecord rec, TSource data, object response)
            : this(rec,data,response,null)
        { }

        public AfterInsertRecordEventArgs(TRecord rec, TSource data, object response, Exception error)
            : base(response, error)
        {
            Record = rec;
            DataObject = data;
        }
    }
}
