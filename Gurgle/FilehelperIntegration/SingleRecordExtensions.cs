using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gurgle
{
    /// <summary>
    /// extension methods for Filehelpers integration
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class SingleRecordExtensions
    {
        public static void ExtractToFile<TRec>(this IRecordSource<TRec> obj, string outPath)
            where TRec : class
        {
            TRec[] recs = obj.ExtractRecords();
            FileHelpers.CommonEngine.WriteFile<TRec>(outPath, recs);
        }

        public static void LoadFile<T>(this IRecordStore<T> obj, string path)
            where T : class
        {
            T[] recs = FileHelpers.CommonEngine.ReadFile<T>(path);
            obj.InsertRecords(recs);
        }
    }
}
