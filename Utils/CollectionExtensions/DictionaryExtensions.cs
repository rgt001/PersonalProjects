using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utils.CollectionExtensions
{
    public static class DictionaryExtensions
    {
        public static DataTable CreateDataTable(this Dictionary<string, PropertyInfo> properties)
        {
            DataTable table = new DataTable();

            foreach (var property in properties)
            {
                table.Columns.Add(property.Key, property.Value.DiscoveryRealType());
            }

            return table;
        }
    }
}
