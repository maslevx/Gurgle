using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace Gurgle
{
    public delegate void MapToRecordHandler<in TData, in TRec>(TRec record, TData data) where TRec : class;
    public delegate void MapFromRecordHandler<in TData, in TRec>(TData data, TRec record) where TRec : class;
    //public delegate void BeforeMakeRecordHandler<TSource, TRec>(ServiceDataStorage<TSource, TRec> source, BeforeMakeRecordEventArgs<TSource> e) where TRec : class;
    //public delegate void BeforeMakeRecordHandler<TData, TRec>(IExtract<TData, TRec> source, BeforeMakeRecordEventArgs<TData> e) where TRec : class;
    //public delegate void AfterInsertRecordHandler<TSource, TRec>(ServiceDataStorage<TSource, TRec> source, AfterInsertRecordEventArgs<TSource, TRec> e) where TRec : class;
    //public delegate void AfterInsertRecordHandler<TData, TRec>(IIntake<TData,TRec> source, AfterInsertRecordEventArgs<TData, TRec> e) where TRec : class;
    
    //multi-record
    public delegate TRec[] MapToRecordsHandler<in TData, out TRec>(TData data) where TRec : class;
    public delegate TData MapFromRecordsHandler<out TData, in TRec>(TRec[] records) where TRec : class;
    //public delegate void BeforeMakeRecordsHandler<TSource, TRec>(MultiRecServiceDataSource<TSource, TRec> source, BeforeMakeRecordEventArgs<TSource> e) where TRec : class;
    //public delegate void BeforeMakeRecordsHandler<TData, TRec>(IMultiRecExtract<TData, TRec> source, BeforeMakeRecordEventArgs<TData> e) where TRec : class;
    //public delegate void BeforeMakeRecordsHandler<TData>(object source, BeforeMakeRecordEventArgs<TData> e);
}
