using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using FileHelpers;

namespace Gurgle
{
    public abstract class DataExtract<TSource, TRec> : IExtract<TSource,TRec> where TRec : class, new()
    {
        private readonly Type m_recordType = typeof (TRec);

        protected abstract IEnumerable<TSource> PullData();

        #region IExtract implement

        public MapToRecordHandler<TSource, TRec> MapToRecordCallback
        { get; set; }
        protected TRec FillRecord(TSource data)
        {
            TRec rtnVal = (TRec)Activator.CreateInstance(m_recordType);

            if (MapToRecordCallback == null)
                throw new InvalidOperationException("MapToRecordCallback is null");

            MapToRecordCallback(rtnVal, data);
            return rtnVal;
        }


        public event EventHandler<BeforeMakeRecordEventArgs<TSource>> BeforeMakeRecord;
        protected bool OnBeforeMakeRecord(TSource data)
        {
            bool rtnVal = false;
            if (BeforeMakeRecord != null)
            {
                BeforeMakeRecordEventArgs<TSource> e = new BeforeMakeRecordEventArgs<TSource>(data);
                BeforeMakeRecord(this, e);
                rtnVal = e.Skip;
            }
            return rtnVal;
        }

        public TRec[] ExtractRecords()
        {
            return this.ToArray(); 
        }


        public IEnumerator<TRec> GetEnumerator()
        {
            foreach (TSource data in PullData())
            {
                bool skip = OnBeforeMakeRecord(data);
                if (skip)
                    continue;

                TRec rec = FillRecord(data);
                yield return rec;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

    }
}