using System;
using System.Globalization;
using System.Reflection;

namespace SOS.Lib.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Get object property
        /// </summary>
        /// <param name="source"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static PropertyInfo GetProperty(this object source, string propertyName)
        { 
            var sourceType = source.GetType();
            return sourceType.GetProperty(propertyName,
                BindingFlags.SetProperty | BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        /// Get property type
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static Type GetPropertyType(this PropertyInfo property)
        {
            var propertyType = property.PropertyType;
            var underlyingType = Nullable.GetUnderlyingType(propertyType);
            return underlyingType ?? propertyType;
        }

        /// <summary>
        ///     Set object property by reflection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public static void SetProperty<T>(this T target, string propertyName, object value)
        {
            var property = target.GetProperty(propertyName);

            if (property == null)
            {
                return;
            }
           
            var targetPropertyType = property.GetPropertyType();

            if (targetPropertyType.IsInstanceOfType(value))
            {
                property.SetValue(target,
                    Convert.ChangeType(value, targetPropertyType,
                        CultureInfo.InvariantCulture));
            }
        }
    }
}