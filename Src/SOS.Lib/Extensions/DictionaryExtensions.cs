using System.Collections.Generic;

namespace SOS.Lib.Extensions;

/// <summary>
/// Dictionary extensions.
/// </summary>
public static class DictionaryExtensions
{
    extension<TK, TV>(IDictionary<TK, TV> dict)
    {
        /// <summary>
        /// Get a value from dictionary or default if the key didn't exist.
        /// </summary>        
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public TV GetValue(TK key, TV defaultValue = default(TV))
        {
            TV value;
            return dict.TryGetValue(key, out value) ? value : defaultValue;
        }
    }
}
