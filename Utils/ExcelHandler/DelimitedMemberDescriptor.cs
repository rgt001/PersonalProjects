using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utils.ExcelHandler.Attributes;

namespace Utils.ExcelHandler
{
    public class DelimitedMemberDescriptor
    {
        public DelimitedMemberDescriptor(PropertyInfo property)
        {
            DelimitedMember attribute = property.GetCustomAttribute<DelimitedMember>(false);
            Property = property;
            Converter = TypeDescriptor.GetConverter(property.PropertyType);
            Index = attribute.Index;
            IndexEnd = attribute.RangeIndex == 0 ? Index : attribute.RangeIndex;
        }

        public bool isRange { get { return IndexEnd > Index; } }

        public PropertyInfo Property { get; set; }

        public TypeConverter Converter { get; set; }

        public int Index { get; set; }

        public int IndexEnd { get; set; }

        public int TotalItems { get { return IndexEnd + 1 - Index; } }
    }
}
