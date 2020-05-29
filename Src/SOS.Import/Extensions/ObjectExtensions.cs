using System;
using System.Globalization;
using System.Reflection;

namespace SOS.Import.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        ///     Set object property by reflection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public static void SetProperty<T>(this T target, string propertyName, object value)
        {
            var targetType = target.GetType();
            var property = targetType.GetProperty(propertyName,
                BindingFlags.SetProperty | BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (property == null)
            {
                return;
            }

            var propertyType = property.PropertyType;
            var underlyingType = Nullable.GetUnderlyingType(propertyType);
            var targetPropertyType = underlyingType ?? propertyType;

            if (targetPropertyType.IsInstanceOfType(value))
            {
                property.SetValue(target,
                    Convert.ChangeType(value, targetPropertyType,
                        CultureInfo.InvariantCulture));
            }
        }
    }
}