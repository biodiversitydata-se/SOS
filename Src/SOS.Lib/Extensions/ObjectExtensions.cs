using Force.DeepCloner;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;


namespace SOS.Lib.Extensions;

public static class ObjectExtensions
{
    private static object GetValueOrExpandoObject(object @object, PropertyInfo property)
    {
        var value = property.GetValue(@object);
        if (value == null) return null;

        var valueType = value.GetType();
        if (valueType.IsValueType || value is string) return value;

        if (value is IEnumerable enumerable) return ToExpandoCollection(enumerable);

        return ToExpando(value);
    }

    private static ExpandoObject ToExpando(object @object)
    {
        var properties = @object.GetType().GetProperties();
        IDictionary<string, object> expando = new ExpandoObject();
        foreach (var property in properties)
        {
            var value = GetValueOrExpandoObject(@object, property);
            expando.Add(property.Name, value);
        }
        return (ExpandoObject)expando;
    }

    private static IEnumerable<ExpandoObject> ToExpandoCollection(IEnumerable enumerable)
    {
        var enumerator = enumerable.GetEnumerator();
        while (enumerator.MoveNext())
        {
            yield return ToExpando(enumerator.Current);
        }
    }

    extension(Type type)
    {
        public bool IsValueTypeOrString()
        {
            return type.IsValueType || type == typeof(string);
        }

        public bool IsIEnumerable()
        {
            return type.IsAssignableTo(typeof(IEnumerable));
        }
    }

    extension(object value)
    {
        public object GetValue(bool asString)
        {
            return value switch
            {
                DateTime dateTime => asString ? dateTime.ToLocalTime().ToString("yyyy-MM-dd hh:mm") : dateTime.ToLocalTime(),
                bool boolean => asString ? boolean.ToString().ToLower() : value,
                _ => value
            };
        }

        public dynamic ToDynamic()
        {
            var expando = ToExpando(value);
            return expando;
        }
    }

    extension(object source)
    {
        /// <summary>
        /// Get object property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public PropertyInfo GetProperty(string propertyName)
        {
            var sourceType = source.GetType();
            return sourceType.GetProperty(propertyName,
                BindingFlags.SetProperty | BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        }
    }

    extension(PropertyInfo property)
    {
        /// <summary>
        /// Get property type
        /// </summary>
        /// <returns></returns>
        public Type GetPropertyType()
        {
            var propertyType = property.PropertyType;
            var underlyingType = Nullable.GetUnderlyingType(propertyType);
            return underlyingType ?? propertyType;
        }
    }

    extension(Type objectType)
    {
        /// <summary>
        /// Check if property exist in object
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="allowObject"></param>
        /// <returns></returns>
        public bool HasProperty(string propertyName, bool allowObject = true)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return false;
            }

            var objectHierarchy = propertyName.Split('.');
            var levels = objectHierarchy?.Length ?? 0;

            var property = objectType.GetProperties().FirstOrDefault(p => p.Name.Equals(objectHierarchy[0], StringComparison.CurrentCultureIgnoreCase));

            // Root objects not are allowed and this is a root object
            if (!allowObject && levels == 1 && (property?.PropertyType?.Namespace?.StartsWith("SOS", StringComparison.CurrentCultureIgnoreCase) ?? false) && !(property?.PropertyType?.IsEnum ?? false))
            {
                return false;
            }

            return levels == 1 ? property != null : property?.PropertyType.HasProperty(string.Join('.', objectHierarchy.Skip(1))) ?? false;
        }
    }

    extension<T>(T target)
    {
        /// <summary>
        ///     Set object property by reflection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public void SetProperty(string propertyName, object value)
        {
            var property = target.GetProperty(propertyName);

            if (property == null)
            {
                return;
            }

            var targetPropertyType = property.GetPropertyType();

            // Make sure decimal separator is dot
            if (targetPropertyType == typeof(double) || targetPropertyType == typeof(float))
            {
                value = value.ToString().Replace(',', '.');
            }

            try
            {
                property.SetValue(target,
                    Convert.ChangeType(value, targetPropertyType,
                        CultureInfo.InvariantCulture));
            }
            catch
            {
                property.SetValue(target,
                    default(T));
            }
        }
    }

    extension<T>(T original) where T : class, new()
    {
        /// <summary>
        /// Creates a deep copy of an object.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <returns></returns>
        public T Clone()
        {
            return original.DeepClone();
        }
    }

    extension<TSource>(IEnumerable<TSource> source)
    {
        #region ConcurrentDictionary
        public ConcurrentDictionary<TKey, TElement> ToConcurrentDictionary<TKey, TElement>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (keySelector == null) throw new ArgumentNullException("keySelector");
            if (elementSelector == null) throw new ArgumentNullException("elementSelector");

            ConcurrentDictionary<TKey, TElement> d = new ConcurrentDictionary<TKey, TElement>(comparer);
            foreach (TSource element in source)
                d.TryAdd(keySelector(element), elementSelector(element));

            return d;
        }

        public ConcurrentDictionary<TKey, TSource> ToConcurrentDictionary<TKey>(Func<TSource, TKey> keySelector)
        {
            return ToConcurrentDictionary<TSource, TKey, TSource>(source, keySelector, IdentityFunction<TSource>.Instance, null);
        }

        public ConcurrentDictionary<TKey, TSource> ToConcurrentDictionary<TKey>(Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            return ToConcurrentDictionary<TSource, TKey, TSource>(source, keySelector, IdentityFunction<TSource>.Instance, comparer);
        }

        public ConcurrentDictionary<TKey, TElement> ToConcurrentDictionary<TKey, TElement>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            return ToConcurrentDictionary<TSource, TKey, TElement>(source, keySelector, elementSelector, null);
        }
    }

    internal class IdentityFunction<TElement>
    {
        public static Func<TElement, TElement> Instance
        {
            get { return x => x; }
        }
    }
    #endregion ConcurrentDictionary
}
