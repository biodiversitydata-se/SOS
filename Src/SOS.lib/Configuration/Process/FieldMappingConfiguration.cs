using System;
using System.Collections.Generic;
using System.Text;

namespace SOS.Lib.Configuration.Process
{
    /// <summary>
    /// Field mapping process configuartion.
    /// </summary>
    public class FieldMappingConfiguration
    {
        /// <summary>
        /// Decides whether field mapping values should be resolved
        /// (for debugging purpose) when processing observations.
        /// </summary>
        public bool ResolveValues { get; set; } = false;

        /// <summary>
        /// Culture code for localized field mapped fields.
        /// </summary>
        public string LocalizationCultureCode { get; set; } = "en-GB";
    }
}
