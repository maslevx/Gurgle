using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gurgle
{
    public sealed class BeforeMakeRecordEventArgs<T>  : EventArgs
    {
        public bool Skip { get; set; }
        public T DataObject { get; private set; }

        public BeforeMakeRecordEventArgs(T data)
        {
            DataObject = data;
            Skip = false;
        }
    }
}
