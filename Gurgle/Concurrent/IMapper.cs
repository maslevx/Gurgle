using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gurgle.Concurrent
{
    public interface ISourceMap<in TSource, out TDestination>
        where TDestination : class
    {
        TDestination[] Map(TSource data);
    }

    public interface ISourceEvaluation<in TSource>
    {
        bool Skip(TSource data);
    }

    public interface IMapper<in TSource, out TRec> : ISourceMap<TSource,TRec>, ISourceEvaluation<TSource>
        where TRec : class
    { }

    public static class IMapperExtensions
    {
        public static Type SourceType<T,R>(this IMapper<T,R> obj) where R : class
        {
            return typeof (T);
        }
        public static Type RecordType<T, R>(this IMapper<T, R> obj) where R : class
        {
            return typeof(R);
        }
    }
}
