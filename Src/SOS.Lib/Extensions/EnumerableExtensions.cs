using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SOS.Lib.Extensions
{
    /// <summary>
    /// Enumerable extensions.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Check if enumerable has any values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool HasItems<T>(this IEnumerable<T> source)
        {
            return (source?.Any() ?? false);
        }

        /// <summary>
        /// Check if enumerable has duplicates
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool HasDuplicates<T>(this IEnumerable<T> source)
        {
            var hs = new HashSet<T>();

            foreach (var item in source)
            {
                if (!hs.Add(item)) return true;
            }

            return false;
        }

        /// <summary>
        /// Cast IEnumerable to read only collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static IReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> enumerable)
        {
            return new ReadOnlyCollection<T>(enumerable?.ToList());
        }

        public static ICollection<T> ToCollection<T>(this IReadOnlyCollection<T> collection)
        {
            return new Collection<T>(collection?.ToList());
        }
    }
}