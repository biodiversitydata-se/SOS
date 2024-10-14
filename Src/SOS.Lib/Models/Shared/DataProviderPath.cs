using System.Collections.Generic;

namespace SOS.Lib.Models.Shared
{
    /// <summary>
    /// Class for data provider path
    /// </summary>
    public class DataProviderPath
    {
        /// <summary>
        /// Culture code i.e. sv-SE
        /// </summary>
        public string CultureCode { get; set; }

        /// <summary>
        /// Localized path
        /// </summary>
        public IEnumerable<string> Path { get; set; }
    }
}
