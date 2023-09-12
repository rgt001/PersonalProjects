using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Attributes
{
    public static class AttributesHandler
    {
        public static List<KeyValuePair<PropertyInfo, T>> GetWithAttribute<T>(this PropertyInfo[] properties) where T : Attribute
        {
            List<KeyValuePair<PropertyInfo, T>> propertiesAttributes = new List<KeyValuePair<PropertyInfo, T>>();
            var type = typeof(T);
            foreach (var property in properties)
            {
                if (property.TryGetAttribute(type, out Attribute attribute))
                {
                    propertiesAttributes.Add(new KeyValuePair<PropertyInfo, T>(property, (T)attribute));
                }
            }

            return propertiesAttributes;
        }

        public static bool TryGetAttribute(this PropertyInfo property, Type attributeType, out Attribute attribute)
        {
            attribute = property.GetCustomAttribute(attributeType);

            if (attribute == null)
                return false;

            return true;
        }
    }
}
