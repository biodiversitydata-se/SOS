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
        /// <returns></returns>
        IDictionary<string, string> Execute(object @object, string prefix = "");
    }
}