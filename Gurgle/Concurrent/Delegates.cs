using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gurgle.Concurrent
{
    public delegate IMapper<TSource, TRec> MapProvider<TSource, TRec>() where TRec : class;
    public delegate void ExtractThreadCompleteHandler<T>(object source, ExtractThreadCompleteEventArgs<T> e);

}
