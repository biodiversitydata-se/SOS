using System.Collections.Generic;

namespace SOS.Lib.Helpers.Interfaces
{
    /// <summary>
    /// Flatten object to dictionary
    /// </summary>
    public interface IObjectFlattenerHelper
    {
        /// <summary>
        /// Flatten object
        /// </summary>
        /// <param name="object"></param>
        /// <param name="prefix"></param>
        /// <param name="valuesAsString"></param>
        /// <returns></returns>
        IDictionary<string, object> Execute(object @object, string prefix = "", bool valuesAsString = false);
    }
}