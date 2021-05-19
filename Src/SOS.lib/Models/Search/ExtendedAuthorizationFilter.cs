using System.Collections.Generic;

namespace SOS.Lib.Models.Search
{
    /// <summary>
    /// Filter for extended authorization
    /// </summary>
    public class ExtendedAuthorizationFilter
    {
        /// <summary>
        /// Parish id's where user has extended authorization
        /// </summary>
        public GeographicAreasFilter GeographicAreas { get; set; }

        /// <summary>
        /// Authorization identity
        /// </summary>
        public string Identity { get; set; }

        /// <summary>
        /// User can see observations of taxa with protection level equal or below this  
        /// </summary>
        public int MaxProtectionLevel { get; set; }

        /// <summary>
        /// Taxa user has extended authorization to see
        /// </summary>
        public IEnumerable<int> TaxonIds { get; set; }
    }
}