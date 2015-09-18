using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gurgle.Concurrent
{
    /// <summary>
    /// just playing with ideas, might make these public later
    /// </summary>
    internal static class Map
    {
        public static IMapper<TSource, TDest> MapperWith<TSource, TDest>(Func<TSource, TDest[]> map,
            Func<TSource, bool> eval)
            where TDest : class
        {
            return new GenericMapper<TSource, TDest>(MapWith(map), EvalWith(eval));
        }

        public static ISourceMap<TSource, TDest> MapWith<TSource, TDest>(Func<TSource, TDest[]> map)
            where TDest : class
        {
            return new GenericMap<TSource, TDest>(map);
        }

        public static IMapper<TSource, TDest> MapWith<TSource, TDest>(this ISourceEvaluation<TSource> eval,
            Func<TSource, TDest[]> map)
            where TDest : class
        {
            return new GenericMapper<TSource, TDest>(MapWith(map), eval);
        }

        public static ISourceEvaluation<T> EvalWith<T>(Func<T, bool> eval)
        {
            return new GenericEval<T>(eval);
        }

        public static IMapper<TSource, TDest> EvalWith<TSource, TDest>(this ISourceMap<TSource, TDest> map,
            Func<TSource, bool> eval)
            where TDest : class
        {
            return new GenericMapper<TSource, TDest>(map, EvalWith(eval));
        }

        #region generic implementations

        class GenericMapper<TSource, TDest> : IMapper<TSource, TDest>
            where TDest : class
        {
            private readonly ISourceMap<TSource, TDest> m_map;
            private readonly ISourceEvaluation<TSource> m_eval;

            public GenericMapper(ISourceMap<TSource, TDest> map, ISourceEvaluation<TSource> eval)
            {
                m_map = map;
                m_eval = eval;
            }

            public TDest[] Map(TSource data)
            {
                return m_map.Map(data);
            }
            
            public bool Skip(TSource data)
            {
                return m_eval.Skip(data);
            }
        }

        class GenericMap<TSource, TDest> : ISourceMap<TSource, TDest>
            where TDest : class
        {
            private readonly Func<TSource, TDest[]> m_mapping;

            public GenericMap(Func<TSource, TDest[]> map)
            {
                m_mapping = map;
            }

            TDest[] ISourceMap<TSource, TDest>.Map(TSource data)
            {
                return m_mapping(data);
            }
        }

        class GenericEval<T> : ISourceEvaluation<T>
        {
            private readonly Func<T, bool> m_eval;

            public GenericEval(Func<T, bool> eval)
            {
                m_eval = eval;
            }

            bool ISourceEvaluation<T>.Skip(T data)
            {
                return m_eval(data);
            }
        }

        #endregion
    }
}
