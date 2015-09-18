using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gurgle
{
    public interface IRecordStore<in TRec>
    {
        void InsertRecord(TRec record);
        //void InsertRecords(TRec[] records);
        void InsertRecords(IEnumerable<TRec> records);
    }

    public interface IMultiRecordStore<TRec> : IMultiRecordBase
    {
       // void InsertRecords<TKey>(TRec[] records, Func<TRec, TKey> selector, IEqualityComparer<TKey> comparer);
       // void InsertRecords<TKey>(TRec[] records, Func<TRec, TKey> selector);

        void InsertRecords<TKey>(IEnumerable<TRec> records, Func<TRec, TKey> selector, IEqualityComparer<TKey> comparer);
        void InsertRecords<TKey>(IEnumerable<TRec> records, Func<TRec, TKey> selector);
    }
}
