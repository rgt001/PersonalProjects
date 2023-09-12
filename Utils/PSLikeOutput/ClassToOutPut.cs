using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utils.PSLikeOutput
{
    public static class ClassToOutPut
    {
        public static IEnumerable<string> TransformIntoOutPut<T>(this IEnumerable<T> source, string request) where T : class
        {
            if (source == null) throw new ArgumentNullException("source");

            if (string.IsNullOrWhiteSpace(request))
            {
                foreach (var item in source)
                    yield return item.ConvertClassToString();

                yield break;
            }

            if (request.Contains('|'))
                request = request.Split('|')[1];

            var properties = GetPropertiesToExport.GetProperties<T>(request);

            if (properties.Count == 0)
                yield break;

            foreach (var item in source)
                yield return item.ToString(properties);
        }

        public static string TransformIntoOutPut<T>(this T source, string request) where T : class
        {
            if (source == null) throw new ArgumentNullException("source");

            if (string.IsNullOrWhiteSpace(request))
                return source.ConvertClassToString();

            if (request.Contains('|'))
                request = request.Split('|')[1];

            var properties = GetPropertiesToExport.GetProperties<T>(request);
            return source.ToString(properties);
        }

        private static string ToString<T>(this T source, List<PropertyInfo> properties) where T : class
        {
            StringBuilder result = new StringBuilder();
            foreach (var property in properties)
            {
                result.Append(property.GetValue(source));
                result.Append(CultureInfo.CurrentCulture.TextInfo.ListSeparator);
            }

            result.Remove(result.Length - CultureInfo.CurrentCulture.TextInfo.ListSeparator.Length, CultureInfo.CurrentCulture.TextInfo.ListSeparator.Length);

            return result.ToString();
        }
    }
}
