using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gurgle
{
    
    public class DataRecord
    {
        private readonly IDictionary m_items;

        protected bool throwIfKeyMissing = false;

        public object this[string key]
        {
            get
            {
                if (!m_items.Contains(key))
                {
                    if (throwIfKeyMissing)
                        throw new ArgumentException("No column for key", "key");
                    return null;
                }
                else
                    return m_items[key];
            }
            set
            {
                if (m_items.Contains(key))
                    m_items[key] = value;
                else
                    m_items.Add(key, value);
            }
        }

        public IEnumerable<string> Fields
        {
            get { return GetFields(); }
        }

        public int FieldCount
        {
            get { return m_items.Count; }
        }

        public DataRecord(IDictionary items)
        {
            if (items == null)
                m_items = new Dictionary<string, object>();
            else
                m_items = new Hashtable(items, StringComparer.InvariantCulture);
        }

        public DataRecord()
            : this(null)
        { }

        public static DataRecord FromReader(IDataReader reader)
        {
            DataRecord rtnVal = new DataRecord();
            for (int i = 0; i < reader.FieldCount; i++)
                rtnVal[reader.GetName(i)] = reader.GetValue(i);

            return rtnVal;
        }

        public T Field<T>(string name)
        {
            try
            {
                return BoxCutter<T>.Open(this[name]);
            }
            catch (InvalidCastException castEx)
            {
                if (this[name] == null || this[name] == DBNull.Value)
                    throw new InvalidCastException(
                        String.Format("Field '{0}' is null, try casting to a nullable type if this is a valid state",
                            name), castEx);
                else
                    throw new InvalidCastException(
                        String.Format("Field '{0}' is of type '{1}' and cannot be cast to type '{2}'", name,
                            this[name].GetType(), typeof (T)), castEx);
            }
        }

        public IEnumerable<string> GetFields()
        {
            foreach (object key in m_items.Keys)
                yield return (string)key;
        }
    }
}
