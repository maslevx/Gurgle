using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gurgle
{
    public abstract class MultiRecDataIntake<TSource,TRec> : IMultiRecIntake<TSource,TRec>
        where TRec : class
    {

        protected abstract void InsertData(TSource data);


        protected MultiRecDataIntake()
        {
            m_recordTypes = new Type[] {typeof (TRec)};
        }

        protected MultiRecDataIntake(params Type[] recordTypes)
        {
            foreach (Type type in recordTypes)
                if (type != typeof(TRec) && !type.IsSubclassOf(typeof(TRec)))
                    throw new Exception(
                        String.Format(
                            "{0} is not assignable to type {1}. All RecordTypes must inherit from TRec: use TRec of 'object' for types that do not share a common parent.",
                            type, typeof(TRec)));

            m_recordTypes = recordTypes;
        }

        #region IIntake implementation

        private readonly Type[] m_recordTypes;
        public Type[] RecordTypes 
        {
            get { return m_recordTypes; }
        }

        public MapFromRecordsHandler<TSource, TRec> MapFromRecordsCallback
        {
            get;
            set;
        }
        protected TSource FillData(TRec[] records)
        {
            if (MapFromRecordsCallback == null)
                throw new InvalidOperationException("MapFromRecordsCallback is null");

            return MapFromRecordsCallback(records);
        }

        public virtual void InsertRecords<TKey>(IEnumerable<TRec> records, Func<TRec, TKey> selector, IEqualityComparer<TKey> comparer)
        {
            IEnumerator<TRec> enumerator = records.GetEnumerator();
            
            if (enumerator.MoveNext() == false) //empty list
                return;

            List<TRec> buffer = new List<TRec>();
            TKey currentKey = selector(enumerator.Current); //first key
            do
            {
                TKey recKey = selector(enumerator.Current);
                if (!comparer.Equals(currentKey, recKey))
                {
                    //add events?
                    TSource data = FillData(buffer.ToArray());
                    InsertData(data);
                    buffer.Clear();
                    currentKey = recKey;
                }
                buffer.Add(enumerator.Current);
            } while (enumerator.MoveNext());

            //get the last set
            InsertData(FillData(buffer.ToArray()));
        }

        public void InsertRecords<TKey>(IEnumerable<TRec> records, Func<TRec, TKey> selector)
        {
            InsertRecords(records, selector, EqualityComparer<TKey>.Default);
        }

        #endregion
    }
}
