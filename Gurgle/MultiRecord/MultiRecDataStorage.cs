using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gurgle
{
    public abstract class MultiRecDataStorage<TData, TRec> : IMultiRecExtract<TData, TRec>, IMultiRecIntake<TData, TRec>
        where TRec : class
    {

        protected abstract MultiRecDataExtract<TData, TRec> Extract { get; }
        protected abstract MultiRecDataIntake<TData, TRec> Intake { get; }


        public abstract Type[] RecordTypes { get; }


        public static implicit operator MultiRecDataExtract<TData, TRec>(MultiRecDataStorage<TData, TRec> obj)
        {
            return obj.Extract;
        }

        public static implicit operator MultiRecDataIntake<TData, TRec>(MultiRecDataStorage<TData, TRec> obj)
        {
            return obj.Intake;
        }


        /* extract */
        public MapToRecordsHandler<TData, TRec> MapToRecordsCallback
        {
            get { return Extract.MapToRecordsCallback; }
            set { Extract.MapToRecordsCallback = value; }
        }

        public event EventHandler<BeforeMakeRecordEventArgs<TData>> BeforeMakeRecords
        {
            add { Extract.BeforeMakeRecords += value; }
            remove { Extract.BeforeMakeRecords -= value; }
        }

        public TRec[] ExtractRecords()
        {
            return Extract.ExtractRecords();
        }


        public IEnumerator<TRec> GetEnumerator()
        {
            return Extract.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        /* intake */
        public MapFromRecordsHandler<TData, TRec> MapFromRecordsCallback
        {
            get { return Intake.MapFromRecordsCallback; }
            set { Intake.MapFromRecordsCallback = value; }
        }

        public void InsertRecords<TKey>(IEnumerable<TRec> records, Func<TRec, TKey> selector, IEqualityComparer<TKey> comparer)
        {
            Intake.InsertRecords(records, selector, comparer);
        }

        public void InsertRecords<TKey>(IEnumerable<TRec> records, Func<TRec, TKey> selector)
        {
            Intake.InsertRecords(records, selector);
        }
        
    }
}
