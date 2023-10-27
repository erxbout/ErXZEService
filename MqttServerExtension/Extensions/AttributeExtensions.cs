using System;
using System.Reflection;

namespace MqttServerExtension.Extensions
{
    internal static class AttributeExtensions
    {
        internal static T GetCustomAttributeOrDefault<T>(this PropertyInfo property) where T : Attribute
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
    }
}
