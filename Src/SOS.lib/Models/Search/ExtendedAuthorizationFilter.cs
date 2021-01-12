using System.Collections.Generic;

namespace SOS.Lib.Models.Search
{
    /// <summary>
    /// Filter for extended authorization
    /// </summary>
    public class ExtendedAuthorizationFilter
    {
        /// <summary>
        /// County id's where user has extended authorization
        /// </summary>
        public IEnumerable<string> CountyIds { get; set; }

        /// <summary>
        /// Municipality id's where user has extended authorization
        /// </summary>
        public IEnumerable<string> MunicipalityIds { get; set; }

        /// <summary>
        /// Province id's where user has extended authorization
        /// </summary>
        public IEnumerable<string> ProvinceIds { get; set; }

        /// <summary>
        /// Taxa user has extended authorization to see
        /// </summary>
        public IEnumerable<int> TaxonIds { get; set; }
    }
}