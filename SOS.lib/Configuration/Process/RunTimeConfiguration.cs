using System.Collections.Generic;

namespace SOS.Lib.Configuration.Process
{
    /// <summary>
    /// Root config
    /// </summary>
    public class RunTimeConfiguration
    {
        /// <summary>
        /// Web servers using the processed data
        /// </summary>
        public IEnumerable<ApplicationConfiguration> Applications { get; set; }
    }
}