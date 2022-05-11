using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;


namespace SOS.Lib.Extensions
{
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

        public static bool IsValueTypeOrString(this Type type)
        {
            return type.IsValueType || type == typeof(string);
        }

        public static object GetValue(this object value, bool asString)
        {
            return value switch
            {
                DateTime dateTime => asString ? dateTime.ToLocalTime().ToString("yyyy-MM-dd hh:mm") : dateTime.ToLocalTime(),
                bool boolean => asString ? boolean.ToString().ToLower() : value,
                _ => value
            };
        }

        public static bool IsIEnumerable(this Type type)
        {
            return type.IsAssignableTo(typeof(IEnumerable));
        }

        

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
        /// Check if property exist in object
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static bool HasProperty(this Type objectType, string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return false;
            }

            var objectHierarchy = propertyName.Split('.');
            var levels = objectHierarchy?.Length ?? 0;

            var property = objectType.GetProperties().FirstOrDefault(p => p.Name.Equals(objectHierarchy[0], StringComparison.CurrentCultureIgnoreCase));

            return levels == 1 ? property != null : property?.PropertyType.HasProperty(string.Join('.', objectHierarchy.Skip(1))) ?? false;
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

        /// <summary>
        /// Creates a deep copy of an object.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="original">Object to copy.</param>
        /// <returns></returns>
        public static T Clone<T>(this T original) where T : class
        {
            return FastDeepCloner.DeepCloner.Clone(original);
        }

        public static dynamic ToDynamic(this object value)
        {            
            var expando = ToExpando(value);
            return expando;
        }

        #region ConcurrentDictionary
        public static ConcurrentDictionary<TKey, TElement> ToConcurrentDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (keySelector == null) throw new ArgumentNullException("keySelector");
            if (elementSelector == null) throw new ArgumentNullException("elementSelector");

            ConcurrentDictionary<TKey, TElement> d = new ConcurrentDictionary<TKey, TElement>(comparer);
            foreach (TSource element in source)
                d.TryAdd(keySelector(element), elementSelector(element));

            return d;
        }

        public static ConcurrentDictionary<TKey, TSource> ToConcurrentDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return ToConcurrentDictionary<TSource, TKey, TSource>(source, keySelector, IdentityFunction<TSource>.Instance, null);
        }

        public static ConcurrentDictionary<TKey, TSource> ToConcurrentDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            return ToConcurrentDictionary<TSource, TKey, TSource>(source, keySelector, IdentityFunction<TSource>.Instance, comparer);
        }

        public static ConcurrentDictionary<TKey, TElement> ToConcurrentDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            return ToConcurrentDictionary<TSource, TKey, TElement>(source, keySelector, elementSelector, null);
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
}
