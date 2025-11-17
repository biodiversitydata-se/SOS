using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SOS.Lib.Extensions;

/// <summary>
/// Enumerable extensions.
/// </summary>
public static class EnumerableExtensions
{    
    extension<T>(IEnumerable<T> source)
    {
        /// <summary>
        /// Check if enumerable has any values
        /// </summary>        
        /// <returns></returns>
        public bool HasItems()
        {
            return (source?.Any() ?? false);
        }

        /// <summary>
        /// Check if enumerable has duplicates
        /// </summary>
        /// <returns></returns>
        public bool HasDuplicates()
        {
            var hs = new HashSet<T>();

            foreach (var item in source)
            {
                if (!hs.Add(item)) return true;
            }

            return false;
        }
    }

    extension<T>(IEnumerable<T> enumerable)
    {
        /// <summary>
        /// Cast IEnumerable to read only collection
        /// </summary>        
        /// <returns></returns>
        public IReadOnlyCollection<T> ToReadOnlyCollection()
        {
            return new ReadOnlyCollection<T>(enumerable?.ToList());
        }
    }

    extension<T>(IReadOnlyCollection<T> collection)
    {
        public ICollection<T> ToCollection()
        {
            return new Collection<T>(collection?.ToList());
        }
    }
}