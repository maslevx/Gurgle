using System.Collections.Generic;

namespace Gurgle
{
    public interface IRecordSource<out TRec> : IEnumerable<TRec>
    {
        TRec[] ExtractRecords();
    }

    public interface IMultiRecordSource<out TRec> : IRecordSource<TRec>, IMultiRecordBase
    {
    }
}