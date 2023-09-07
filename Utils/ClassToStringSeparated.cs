using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public static class ClassToStringSeparated
    {
        public static string ConvertClassToString(this object source, Type classType)
        {
            StringBuilder result = new StringBuilder();
            var properties = classType.GetProperties();

            foreach (var property in properties)
            {
                result.Append(property.GetValue(source));
                result.Append(CultureInfo.CurrentCulture.TextInfo.ListSeparator);
            }

            return result.ToString();
        }
    }
}
