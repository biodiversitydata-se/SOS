using System.Collections.Generic;

namespace SOS.Lib.Helpers.Interfaces
{
    /// <summary>
    /// Flatten object to dictionary
    /// </summary>
    public interface IObjectHelper
    {
        /// <summary>
        /// Flatten object
        /// </summary>
        /// <param name="object"></param>
        /// <param name="prefix"></param>
        /// <param name="valuesAsString"></param>
        /// <returns></returns>
        IDictionary<string, object> Flatten(object @object, string prefix = "", bool valuesAsString = false);

        /// <summary>
        /// Get fileds containg garbage chars
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="object"></param>
        /// <returns></returns>
        IEnumerable<string> GetPropertiesWithGarbageChars<T>(T @object);
    }
}