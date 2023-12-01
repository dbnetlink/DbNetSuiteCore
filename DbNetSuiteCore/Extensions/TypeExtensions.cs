using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace DbNetSuiteCore.Extensions
{
    public static class TypeExtension
    {
        public static Dictionary<string,Type> PropertyTypes(this Type type)
        {
            Dictionary<string, Type> propertyTypes = new Dictionary<string, Type>();

            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (!property.CanRead || IsSimple(property.PropertyType) == false)
                {
                    continue;
                }

                propertyTypes.Add(property.Name, property.PropertyType);
            }

            return propertyTypes;
        }

        private static bool IsSimple(Type type)
        {
            return TypeDescriptor.GetConverter(type).CanConvertFrom(typeof(string));
        }
    }
}