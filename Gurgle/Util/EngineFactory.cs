using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FileHelpers;

namespace Gurgle
{
    internal class EngineFactory
    {
        public static FileHelperEngine GetEngineForType(Type recordType)
        {
            FileHelperEngine rtnVal;
            if (recordType.GetCustomAttributes(false).Count(p => p.GetType() == typeof(DelimitedRecordAttribute)) == 1)
                rtnVal = new DelimitedFileEngine(recordType);
            else if (recordType.GetCustomAttributes(false).Count(p => p.GetType() == typeof(FixedLengthRecordAttribute)) == 1)
                rtnVal = new FixedFileEngine(recordType);
            else
                throw new InvalidOperationException(String.Format("Record type {0} is not a Filehelpers class", recordType));

            return rtnVal;
        }
    }
}
