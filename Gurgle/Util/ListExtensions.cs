using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gurgle.Utils
{
    public static class ListExtensions
    {
        public static bool HasAny<T>(this IEnumerable<T> source)
        {
            return source != null && source.Any();
        }


        public enum SplitMode
        {
            AllowMore,
            AllowLess,
            Exact
        }
        public static IList<IList<T>> SplitList<T>(this IList<T> source, int blockSize)
        {
            return source.SplitList(blockSize, SplitMode.Exact);
        }
        public static IList<IList<T>> SplitList<T>(this IList<T> source, int blockSize, SplitMode mode)
        {
            IList<IList<T>> rtnVal = new List<IList<T>>();

            T[] tmp = new T[blockSize];
            int c = 0;
            for (int i = 0; i < source.Count; i++)
            {
                tmp[c] = source[i];
                if (++c == blockSize)
                {
                    rtnVal.Add(new List<T>(tmp));
                    c = 0;
                }
            }
            if (c > 0)
            {
                if (rtnVal.Count == 0)
                    rtnVal.Add(new List<T>(tmp.Take(c)));
                else
                {
                    switch (mode)
                    {
                        case SplitMode.AllowLess:
                            rtnVal.Add(new List<T>(tmp.Take(c)));
                            break;
                        case SplitMode.AllowMore:
                            ((List<T>)rtnVal[rtnVal.Count - 1]).AddRange(new List<T>(tmp.Take(c)));
                            break;
                        case SplitMode.Exact:
                            //leftover elements are lost
                            break;
                    }
                }
            }

            return rtnVal;
        }
    }
}
