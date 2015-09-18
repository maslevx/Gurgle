using System;

namespace Gurgle
{
    public interface IExtract<TData, TRec> : IRecordSource<TRec>
        where TRec : class
    {
        MapToRecordHandler<TData, TRec> MapToRecordCallback
        { get; set; }
        
       // event BeforeMakeRecordHandler<TData, TRec> BeforeMakeRecord;
        event EventHandler<BeforeMakeRecordEventArgs<TData>> BeforeMakeRecord;
    }
}