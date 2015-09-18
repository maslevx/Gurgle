using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gurgle
{
    /// <summary>
    /// utility class to unbox variables
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal static class BoxCutter<T>
    {
        public static Converter<object, T> Open = GetConverter(typeof(T));

        private static Converter<object, T> GetConverter(Type t)
        {
            if (t.IsValueType == false)
                return RefTypeConverter;

            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                return (Converter<object, T>)Delegate.CreateDelegate(typeof(Converter<object, T>),
                            typeof(BoxCutter<T>).GetMethod("NullableConverter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                                .MakeGenericMethod(t.GetGenericArguments()[0]));


            return ValueTypeConverter;
        }

        private static T RefTypeConverter(object from)
        {
            if (from == null || from == DBNull.Value)
                return default(T);
            else
                return (T)from;
        }

        private static T ValueTypeConverter(object from)
        {
            if (from == null || from == DBNull.Value)
                throw new InvalidCastException("Cannot cast null to value type " + typeof(T).ToString());

            return (T)from;
        }

        
// ReSharper disable once UnusedMember.Local
        private static TNull? NullableConverter<TNull>(object from)
            where TNull : struct
        {
            if (from == null || from == DBNull.Value)
                return default(Nullable<TNull>);

            return new Nullable<TNull>((TNull)from);
        }
    }
}
