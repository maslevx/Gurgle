using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHelpers;
namespace Gurgle
{
    /// <summary>
    /// extensions for Filehelpers integration
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class MultiRecordExtensions
    {
        public static void ExtractToFile<TRec>(this IMultiRecordSource<TRec> obj, string outPath)
            where TRec : class
        {
            List<object> recs = new List<object>();
            recs.AddRange(obj.ExtractRecords());
            WriteRecords(outPath, obj.RecordTypes, recs);
        }

        public static void LoadFile<TRec, TKey>(this IMultiRecordStore<TRec> obj, string path, Func<TRec, TKey> selector,
            IEqualityComparer<TKey> comparer)
            where TRec : class
        {
            TRec[] recs = ReadRecords<TRec>(path, obj.RecordTypes);
            obj.InsertRecords(recs, selector, comparer);
        }

        public static void LoadFile<TRec, TKey>(this IMultiRecordStore<TRec> obj, string path, Func<TRec, TKey> selector)
            where TRec : class
        {
            TRec[] recs = ReadRecords<TRec>(path, obj.RecordTypes);
            obj.InsertRecords(recs, selector);
        }

        private static void WriteRecords(string path, Type[] recTypes, IEnumerable<object> recs)
        {
            if (recTypes.Length > 1)
            {
                MultiRecordEngine multEngine = new MultiRecordEngine(recTypes); //for some reason multirecengine doesn't seem to share a common interface with single records
                multEngine.WriteFile(path, recs);
            }
            else
            {
                FileHelperEngine engine = new FileHelperEngine(recTypes.First());
                engine.WriteFile(path, recs);
            }
        }

        private static T[] ReadRecords<T>(string path, Type[] recTypes)
        {
            T[] rtnVal;
            if (recTypes.Length > 1)
            {
                MultiRecordEngine multEngine = new MultiRecordEngine(recTypes);
                rtnVal = multEngine.ReadFile(path).Cast<T>().ToArray();
            }
            else
            {
                FileHelperEngine engine = new FileHelperEngine(recTypes.First());
                rtnVal = engine.ReadFile(path).Cast<T>().ToArray();
            }

            return rtnVal;
        }
    }
}
