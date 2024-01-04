using SOS.Lib.Configuration.Shared;
using System.Collections.Generic;

namespace SOS.Lib.Configuration.Import
{
    public class SharkServiceConfiguration : RestServiceConfiguration
    {
        /// <summary>
        /// Data types to handle
        /// </summary>
        public IEnumerable<string> ValidDataTypes { get; set; }
    }
}