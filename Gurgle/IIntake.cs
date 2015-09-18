using System;

namespace Gurgle
{
    public interface IIntake<TData,TRec,TEventArg> : IRecordStore<TRec>
        where TRec : class
        where TEventArg : AfterPostEventBase
    {
        bool ContinueOnError { get; set; }

        MapFromRecordHandler<TData, TRec> MapFromRecordCallback
        { get; set; }

        //event AfterInsertRecordHandler<TData, TRec> AfterInsertRecord;
        event EventHandler<TEventArg> AfterInsertRecord;

        //void InsertRecord(TRec record);
        //void InsertRecords(TRec[] records);
    }
}