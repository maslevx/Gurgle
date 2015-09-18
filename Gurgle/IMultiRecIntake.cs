using System;
using System.Collections.Generic;

namespace Gurgle
{
    public interface IMultiRecIntake<TData,TRec> : IMultiRecordStore<TRec>
        where TRec : class
    {
        MapFromRecordsHandler<TData, TRec> MapFromRecordsCallback
        { get; set; }
        
    }
}