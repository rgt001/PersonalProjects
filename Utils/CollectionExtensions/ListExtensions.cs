using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utils.CollectionExtensions
{
    public static class ListExtensions
    {
        public static IEnumerable<List<T>> PartitionFor<T>(this IList<T> input, int blocksQuantity)
        {
            int blockSize = input.Count / blocksQuantity;
            var enumerator = input.GetEnumerator();

            while (enumerator.MoveNext())
            {
                yield return NextPartition(enumerator, blockSize).ToList();
            }
        }

        public static IEnumerable<List<T>> Partition<T>(this IEnumerable<T> input, int blockSize)
        {
            var enumerator = input.GetEnumerator();

            while (enumerator.MoveNext())
            {
                yield return NextPartition(enumerator, blockSize).ToList();
            }
        }

        private static IEnumerable<T> NextPartition<T>(IEnumerator<T> enumerator, int blockSize)
        {
            do
            {
                yield return enumerator.Current;
            }
            while (--blockSize > 0 && enumerator.MoveNext());
        }

        public static DataTable ToDataTable<T>(this IList<T> data)
        {
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                                        .Where(p => p.IsDefined(typeof(ColumnAttribute)) && !p.IsDefined(typeof(NotMappedAttribute))).ToArray();
            DataTable table = new DataTable();

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];
                table.Columns.Add(property.Name, property.DiscoveryRealType());
            }

            object[] values = new object[table.Columns.Count];

            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    if (properties[i].PropertyType.IsGenericType && properties[i].PropertyType.GetGenericTypeDefinition() == typeof(Lazy<>))
                    {
                        var lazy = properties[i].GetValue(item);
                        if (lazy != null)
                            values[i] = lazy.GetType().GetProperty(nameof(Lazy<object>.Value)).GetValue(lazy);
                    }
                    else
                    {
                        values[i] = properties[i].GetValue(item);
                    }
                }
                table.Rows.Add(values);
            }

            return table;
        }


    }
}
