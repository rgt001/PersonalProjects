using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.ExcelHandler.Attributes;

namespace Utils.ExcelHandler
{
    public abstract class OpenBase
    {
        public IEnumerable<string> Open(string path, bool hasReader = true)
        {
            foreach (var rowItems in InternalOpen(path, hasReader))
            {
                yield return string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, rowItems);
            }
        }

        public IEnumerable<T> Open<T>(string path, bool hasReader = true) where T : new()
        {
            Dictionary<int, DelimitedMemberDescriptor> propertiesIndexed = new Dictionary<int, DelimitedMemberDescriptor>();

            var properties = typeof(T).GetProperties().Where(p => p.IsDefined(typeof(DelimitedMember), false)).ToList();

            int count = 1;
            foreach (var property in properties)
            {
                propertiesIndexed.Add(count++, new DelimitedMemberDescriptor(property));
            }

            //int interactions = propertiesIndexed.OrderBy(p => p.Value.IndexEnd).Last().Value.IndexEnd;

            foreach (var row in InternalOpen(path, hasReader))
            {
                var enumerator = row.GetEnumerator();
                T instance = (T)Activator.CreateInstance(typeof(T));

                for (int i = 1; i <= properties.Count; i++)
                {
                    if (propertiesIndexed[i].isRange)
                    {
                        object[] values = new object[propertiesIndexed[i].TotalItems];
                        for (int r = 0; r < propertiesIndexed[i].TotalItems; r++)
                        {
                            enumerator.MoveNext();
                            values[r] = enumerator.Current;
                        }

                        var array = propertiesIndexed[i].Property.GetValue(instance);
                        array.SetValuesToArray(values);
                    }
                    else
                    {
                        enumerator.MoveNext();
                        propertiesIndexed[i].Property.SetValue(instance, propertiesIndexed[i].Converter.ConvertFromString(null, CultureInfo.InvariantCulture, enumerator.Current.TrimEnd()), null);
                    }
                }

                yield return instance;
            }
        }

        public abstract IEnumerable<IEnumerable<string>> InternalOpen(string path, bool hasReader = true);
    }
}
