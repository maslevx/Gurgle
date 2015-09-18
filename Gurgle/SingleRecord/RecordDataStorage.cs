using System;
using System.Collections.Generic;

namespace Gurgle
{
    public abstract class RecordDataStorage<TData,TRec,TIntakePostEvent> : IExtract<TData,TRec>, IIntake<TData,TRec,TIntakePostEvent> 
        where TRec : class, new()
        where TData : class, new() where TIntakePostEvent : AfterPostEventBase
    {

        protected abstract DataExtract<TData, TRec> Extract { get; }
        protected abstract DataIntake<TData, TRec, TIntakePostEvent> Intake { get; }

        public static implicit operator DataExtract<TData, TRec>(RecordDataStorage<TData, TRec,TIntakePostEvent> obj)
        {
            return obj.Extract;
        }

        public static implicit operator DataIntake<TData, TRec,TIntakePostEvent>(RecordDataStorage<TData, TRec,TIntakePostEvent> obj)
        {
            return obj.Intake;
        }


        /* IExtract */
        public MapToRecordHandler<TData, TRec> MapToRecordCallback
        {
            get { return Extract.MapToRecordCallback; }
            set { Extract.MapToRecordCallback = value; }
        }

        public event EventHandler<BeforeMakeRecordEventArgs<TData>> BeforeMakeRecord
        {
            add { Extract.BeforeMakeRecord += value; }
            remove { Extract.BeforeMakeRecord -= value; }
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

        //public void ExtractToFile(string OutputPath)
        //{
        //    Extract.ExtractToFile(OutputPath);
        //}

        /* IIntake */

        public bool ContinueOnError
        {
            get { return Intake.ContinueOnError; }
            set { Intake.ContinueOnError = value; }
        }

        public MapFromRecordHandler<TData, TRec> MapFromRecordCallback
        {
            get { return Intake.MapFromRecordCallback; }
            set { Intake.MapFromRecordCallback = value; }
        }

        public event EventHandler<TIntakePostEvent> AfterInsertRecord
        {
            add { Intake.AfterInsertRecord += value; }
            remove { Intake.AfterInsertRecord -= value; }
        }

        public void InsertRecord(TRec record)
        {
            Intake.InsertRecord(record);
        }

        public void InsertRecords(IEnumerable<TRec> records)
        {
            Intake.InsertRecords(records);
        }
    }
}