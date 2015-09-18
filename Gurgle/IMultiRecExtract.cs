using System;

namespace Gurgle
{
    public interface IMultiRecExtract<TData,TRec> : IMultiRecordSource<TRec>
        where TRec : class
    {
        MapToRecordsHandler<TData, TRec> MapToRecordsCallback
        { get; set; }

       // event BeforeMakeRecordsHandler<TData, TRec> BeforeMakeRecords;
        event EventHandler<BeforeMakeRecordEventArgs<TData>> BeforeMakeRecords;
    }
}