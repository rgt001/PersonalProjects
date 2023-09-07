using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public static class StringExtension
    {
        public static int WordCount(this string str)
        {
            if (str is null)
                throw new ArgumentNullException(nameof(str));

            return str.Split(new char[] { ' ', '.', '?' },
                             StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public static string ReplaceEmpty(this string str, params string[] replaces)
        {
            if (str is null)
                throw new ArgumentNullException(nameof(str));
            if (replaces is null)
                throw new ArgumentNullException(nameof(replaces));

            foreach (var replace in replaces)
            {
                str = str.Replace(replace, "");
            }

            return str;
        }

        public static bool IsAnyEmpty(params string[] parameters)
        {
            foreach (var item in parameters)
            {
                if (string.IsNullOrWhiteSpace(item))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsAllEmpty(params string[] parameters)
        {
            foreach (var item in parameters)
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    return false;
                }
            }
            return true;
        }

        public static string ReplaceEmpty(this string str, IList<string> replaces)
        {
            if (str is null)
                throw new ArgumentNullException(nameof(str));
            if (replaces is null)
                throw new ArgumentNullException(nameof(replaces));

            foreach (var replace in replaces)
            {
                str = str.Replace(replace, "");
            }

            return str;
        }

        public static string SafeToString(this object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            return obj.ToString();
        }

        public static bool IsnullEmpty(this ICollection<string> variaveis)
        {
            if (variaveis is null)
                throw new ArgumentNullException(nameof(variaveis));

            foreach (var var in variaveis)
            {
                if (!string.IsNullOrEmpty(var))
                    return false;
            }

            return true;
        }

        public static int OccurrencesCount(this string source, char target)
        {
            int count = 0;

            foreach (var character in source)
            {
                if (character == target) count++;
            }

            return count;
        }

        public static string TakeRight(this string source, string startAt, bool end = true)
        {
            int index = source.IndexOf(startAt);
            if (end)
                index += startAt.Length;

            return source.Substring(index);
        }

        public static string TakeLeft(this string source, string endAt)
        {
            int index = source.IndexOf(endAt);
            return source.Substring(0, index);
        }
    }
}
