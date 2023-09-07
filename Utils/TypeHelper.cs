using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Utils
{
    public static class TypeHelper
    {
        private static readonly char[] NullDate = new char[] { '0', '0', '0', '1', '-', '0', '1', '-', '0', '1' };

        public static bool TryGetSqlType(this Type type, out SqlDbType sqlType)
        {
            if (type == typeof(int) || type == typeof(int?))
                sqlType = SqlDbType.Int;
            else if (type == typeof(string))
                sqlType = SqlDbType.VarChar;
            else if (type == typeof(DateTime) || type == typeof(DateTime?))
                sqlType = SqlDbType.DateTime;
            else if (type == typeof(DateTimeOffset))
                sqlType = SqlDbType.DateTimeOffset;
            else if (type == typeof(double) || type == typeof(double?))
                sqlType = SqlDbType.Float;
            else if (type == typeof(char))
                sqlType = SqlDbType.Char;
            else if (type == typeof(bool))
                sqlType = SqlDbType.Bit;
            else if (type == typeof(Guid) || type == typeof(Guid?))
                sqlType = SqlDbType.UniqueIdentifier;
            else if (type == typeof(byte[]))
                sqlType = SqlDbType.VarBinary;
            else//Margin for error, if it's an object, one should know how to handle and deliver the correct value
            {
                sqlType = SqlDbType.Variant;
                return false;
            }

            return true;
        }

        public static SqlDbType AsSqlType(this Type type)
        {
            return GetSqlType(type);
        }

        public static SqlDbType GetSqlType(this Type type)
        {
            TryGetSqlType(type, out SqlDbType sqlType);
            return sqlType;
        }

        public static Type DiscoveryRealType(this PropertyInfo propertyInfo)
        {
            var propertyType = propertyInfo.PropertyType;
            if (propertyType.IsGenericType)//If it's a generic class, get generic type
            {
                return propertyType.GenericTypeArguments[0];
            }

            return propertyInfo.PropertyType;
        }

        public static char AsBit(this object source)
        {
            if (source.ToString() == "True")
                return '1';

            return '0';
        }

        public static string AsDateTime(this object source)
        {
            return AsSqlDateTime(source);
        }

        public static string AsSqlDateTime(this object source)
        {
            //0123456789|12345678
            //30/12/2016 00:00:00
            //0123456789|123456789|12345
            //20/11/2019 19:01:05 +00:00
            var value = source.ToString().ToArray();
            char[] date = new char[] { value[6], value[7], value[8], value[9], '-', value[3], value[4], '-', value[0], value[1] };
            date.CopyTo(value, 0);

            if (CharArrayEquals(date, NullDate))
                return null;

            return new string(value);
        }

        private static bool CharArrayEquals(char[] a1, char[] a2)
        {
            for (var i = 0; i < a1.Length; i++)
            {
                if (a1[i] != a2[i])
                    return false;
            }

            return true;
        }
    }
}
