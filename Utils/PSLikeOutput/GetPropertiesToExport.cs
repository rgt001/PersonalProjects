using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utils.Attributes;

namespace Utils.PSLikeOutput
{
    public static class GetPropertiesToExport
    {
        private const string select = "Select ";
        private static readonly char[] splitCharacters = new char[] { ',', ';' };
        public static List<PropertyInfo> GetProperties<T>(string inputText) where T : class
        {
            if (inputText == null) throw new ArgumentNullException(nameof(inputText));

            if (inputText.Length == 0) return new List<PropertyInfo>(0);

            if (inputText.TrimStart(' ').StartsWith(select, StringComparison.InvariantCultureIgnoreCase))
            {
                var requestedColumns = inputText.ToLower().Remove(0, select.Length).Split(splitCharacters, StringSplitOptions.RemoveEmptyEntries);
                var properties = typeof(T).GetProperties().GetWithAttribute<PossibleNamesAttribute>();
                List<PropertyInfo> requestedProperties = properties.Where(p => p.Value.Name.Any(pp => requestedColumns.Contains(pp))).Select(p => p.Key).ToList();
                return requestedProperties;
            }

            return new List<PropertyInfo>(0);
        }
    }
}
