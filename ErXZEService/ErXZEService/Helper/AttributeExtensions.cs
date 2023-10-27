using System;
using System.Reflection;

namespace ErXZEService.Helper
{
    public static class AttributeExtensions
    {
        public static Attribute GetCustomAttributeOrDefault(this PropertyInfo property, Type attributeType)
        {
            try
            {
                return property.GetCustomAttribute(attributeType);
            }
            catch
            {
                return null;
            }
        }

        public static T GetCustomAttributeOrDefault<T>(this PropertyInfo property) where T : Attribute
        {
            try
            {
                return property.GetCustomAttribute<T>();
            }
            catch
            {
                return null;
            }
        }

        public static Attribute GetCustomAttributeOrDefault(this Type type, Type attributeType)
        {
            try
            {
                return type.GetCustomAttribute(attributeType);
            }
            catch
            {
                return null;
            }
        }
    }
}
