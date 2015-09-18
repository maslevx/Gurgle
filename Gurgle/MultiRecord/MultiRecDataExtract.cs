using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gurgle
{
    public abstract class MultiRecDataExtract<TSource, TRec> : IMultiRecExtract<TSource, TRec>
        where TRec : class
    {

        protected abstract IEnumerable<TSource> PullData();

        protected MultiRecDataExtract()
        {
            m_recordTypes = new Type[] {typeof (TRec)};
        }

        protected MultiRecDataExtract(params Type[] recordTypes)
        {
            foreach(Type type in recordTypes)
                if (type != typeof (TRec) && !type.IsSubclassOf(typeof (TRec)))
                    throw new Exception(
                        String.Format(
                            "{0} is not assignable to type {1}. All RecordTypes must inherit from TRec: use TRec of 'object' for types that do not share a common parent.",
                            type, typeof (TRec)));

            m_recordTypes = recordTypes;
        }

        #region IExtract implementation

        private readonly Type[] m_recordTypes;
        public Type[] RecordTypes
        {
            get { return m_recordTypes; }
        }

        public MapToRecordsHandler<TSource, TRec> MapToRecordsCallback
        {
            get;
            set;
        }
        protected TRec[] FillRecords(TSource data)
        {
            if (MapToRecordsCallback == null)
                throw new InvalidOperationException("MapToRecordsCallback is null");

            return MapToRecordsCallback(data);
        }

        public event EventHandler<BeforeMakeRecordEventArgs<TSource>> BeforeMakeRecords;
        protected bool OnBeforeMakeRecords(TSource data)
        {
            bool rtnVal = false;
            if (BeforeMakeRecords != null)
            {
                BeforeMakeRecordEventArgs<TSource> e = new BeforeMakeRecordEventArgs<TSource>(data);
                BeforeMakeRecords(this, e);
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
                bool skip = OnBeforeMakeRecords(data);
                if (skip)
                    continue;

                TRec[] recs = FillRecords(data);
                foreach (TRec rec in recs)
                    yield return rec;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        
    }
}
