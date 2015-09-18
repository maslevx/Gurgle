using System;

namespace Gurgle
{
    public interface IMultiRecordBase
    {
        Type[] RecordTypes { get; }
    }
}