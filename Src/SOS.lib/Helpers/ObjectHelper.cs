using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers.Interfaces;

namespace SOS.Lib.Helpers
{
    /// <inheritdoc />
    public class ObjectHelper : IObjectHelper
    {
        private static readonly ConcurrentDictionary<Type, Dictionary<PropertyInfo, Func<object, object>>> CachedProperties;
        private static readonly Regex RxIllegalCharacters; // Match all control characters and other non-printable characters

        private static bool ContainsIllegalCharacters(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            return RxIllegalCharacters.Match(value).Success;
        }

        private static Dictionary<string, object> ExecuteInternal(
            object @object,
            Dictionary<string, object> dictionary = default,
            string prefix = "",
            bool valuesAsString = false)
        {
            dictionary ??= new Dictionary<string, object>();
            var type = @object.GetType();
            var properties = GetTypeProperties(type);

            foreach (var (property, getter) in properties)
            {
                var key = string.IsNullOrWhiteSpace(prefix) ? property.Name : $"{prefix}.{property.Name}";
                var value = getter(@object);

                if (value == null)
                {
                    // dictionary.Add(key, null);
                    continue;
                }

                if (property.PropertyType.IsValueTypeOrString())
                {
                    dictionary.Add(key, value.GetValue(valuesAsString));
                }
                else
                {
                    if (value is IEnumerable enumerable)
                    {
                        var counter = 0;
                        foreach (var item in enumerable)
                        {
                            var itemKey = $"{key}[{counter++}]";
                            var itemType = item.GetType();
                            if (itemType.IsValueTypeOrString())
                            {
                                dictionary.Add(itemKey, item.GetValue(valuesAsString));
                            }
                            else
                            {
                                ExecuteInternal(item, dictionary, itemKey, valuesAsString);
                            }
                        }
                    }
                    else
                    {
                        ExecuteInternal(value, dictionary, key, valuesAsString);
                    }
                }
            }

            return dictionary;
        }

        /// <summary>
        /// Get properties for passed object type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Dictionary<PropertyInfo, Func<object, object>> GetTypeProperties(Type type)
        {
            if (CachedProperties.TryGetValue(type, out var properties))
            {
                return properties;
            }

            CacheProperties(type);
            return CachedProperties[type];
        }
        private static void CacheProperties(Type type)
        {
            if (CachedProperties.ContainsKey(type))
            {
                return;
            }

            CachedProperties[type] = new Dictionary<PropertyInfo, Func<object, object>>();
            var properties = type.GetProperties().Where(x => x.CanRead);
            foreach (var propertyInfo in properties)
            {
                var getter = CompilePropertyGetter(propertyInfo);
                CachedProperties[type].Add(propertyInfo, getter);
                if (!propertyInfo.PropertyType.IsValueTypeOrString())
                {
                    if (propertyInfo.PropertyType.IsIEnumerable())
                    {
                        var types = propertyInfo.PropertyType.GetGenericArguments();
                        foreach (var genericType in types)
                        {
                            if (!genericType.IsValueTypeOrString())
                            {
                                CacheProperties(genericType);
                            }
                        }
                    }
                    else
                    {
                        CacheProperties(propertyInfo.PropertyType);
                    }
                }
            }
        }

        private static Func<object, object> CompilePropertyGetter(PropertyInfo property)
        {
            var objectType = typeof(object);
            var objectParameter = Expression.Parameter(objectType);
            var castExpression = Expression.TypeAs(objectParameter, property.DeclaringType);
            var convertExpression = Expression.Convert(
                Expression.Property(castExpression, property),
                objectType);
            return Expression.Lambda<Func<object, object>>(
                convertExpression,
                objectParameter).Compile();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        static ObjectHelper()
        {
            CachedProperties = new ConcurrentDictionary<Type, Dictionary<PropertyInfo, Func<object, object>>>();
            RxIllegalCharacters = new Regex(@"\p{C}+", RegexOptions.Compiled);
        }

        /// <inheritdoc />
        public IDictionary<string, object> Flatten(object @object, string prefix = "", bool valuesAsString = false)
        {
            return ExecuteInternal(@object, prefix: prefix, valuesAsString: valuesAsString);
        }

        public IEnumerable<string> GetPropertiesWithGarbageChars<T>( T @object)
        {
            var garbageProperties = new HashSet<string>();

            if (@object == null)
            {
                return garbageProperties;
            }

            var objectType = @object.GetType();
            var properties = GetTypeProperties(objectType);

            foreach (var (property, getter) in properties)
            {
                var key = property.Name;
                var value = getter(@object);

                if (value == null)
                {
                    // dictionary.Add(key, null);
                    continue;
                }
                var properyType = property.PropertyType;
                if (properyType.IsValueTypeOrString())
                {
                    if (properyType == typeof(string))
                    {
                        if (ContainsIllegalCharacters(value.ToString()))
                        {
                            garbageProperties.Add(property.Name);
                        }
                    }
                    continue;
                }

                if (value is IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        var itemType = item.GetType();
                        if (itemType.IsValueTypeOrString())
                        {
                            if (itemType == typeof(string))
                            {
                                if (ContainsIllegalCharacters(item?.ToString()))
                                {
                                    garbageProperties.Add(property.Name);
                                }
                            }
                            continue;
                        }
                        else
                        {
                            var subObjProperties = GetPropertiesWithGarbageChars(item);

                            if (subObjProperties != null)
                            {
                                foreach (var subPropertie in subObjProperties)
                                {
                                    garbageProperties.Add($"{property.Name}.{subPropertie}");
                                }
                            }
                        }
                    }

                    continue;
                }

                var subProperties = GetPropertiesWithGarbageChars(property.GetValue(@object, null));

                if (subProperties != null)
                {
                    foreach (var subPropertie in subProperties)
                    {
                        garbageProperties.Add($"{property.Name}.{subPropertie}");
                    }
                }
            }

            return garbageProperties;
        }
    }
}
