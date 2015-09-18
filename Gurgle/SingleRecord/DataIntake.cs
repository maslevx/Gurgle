using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gurgle
{
    public abstract class DataIntake<TData, TRec,TEventArg> : IIntake<TData, TRec,TEventArg>
        where TRec : class
        where TData : class, new()
        where TEventArg : AfterPostEventBase
    {

        private readonly Type m_dataType = typeof (TData);

        protected abstract object InsertData(TData data);

        protected abstract TEventArg AfterInsertEventArg(TData data, TRec rec, object response = null, Exception error = null);

        #region IIntake implement

        public bool ContinueOnError { get; set; }

        public MapFromRecordHandler<TData, TRec> MapFromRecordCallback { get; set; }

        protected TData FillData(TRec rec)
        {
            TData rtnVal = (TData)Activator.CreateInstance(m_dataType);

            if (MapFromRecordCallback == null)
                throw new InvalidOperationException("MapFromRecordCallback is null");

            MapFromRecordCallback(rtnVal, rec);
            return rtnVal;
        }

        public event EventHandler<TEventArg> AfterInsertRecord;

        protected void OnAfterInsertRecord(TData data, TRec rec, object response = null, Exception error = null)
        {
            if (AfterInsertRecord != null)
            {
                AfterInsertRecord(this, AfterInsertEventArg(data, rec, response, error));
            }
        }


        public void InsertRecord(TRec record)
        {
            TData tmp = FillData(record);
            object resp = null;
            
            try
            {
                resp = InsertData(tmp);
                OnAfterInsertRecord(tmp, record, resp);
            }
            catch (Exception e)
            {
                if (ContinueOnError)
                    OnAfterInsertRecord(tmp, record, resp, e);
                else
                    throw;
            }

        }

        //public virtual void InsertRecords(TRec[] records)
        //{
        //    InsertRecords((IEnumerable<TRec>)records);
        //}
        public virtual void InsertRecords(IEnumerable<TRec> records)
        {
            foreach (TRec rec in records)
                InsertRecord(rec);
        }
        #endregion


        
    }
}
