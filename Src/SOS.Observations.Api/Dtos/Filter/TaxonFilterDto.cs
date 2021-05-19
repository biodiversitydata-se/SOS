using System.Collections.Generic;

namespace SOS.Observations.Api.Dtos.Filter
{
    /// <summary>
    /// Taxon filter.
    /// </summary>
    public class TaxonFilterDto : TaxonFilterBaseDto
    {
        /// <summary>
        /// Redlist categories to match.
        /// Possible values are: "DD", "EX", "RE", "CR", "EN", "VU", "NT", "LC", "NA", "NE"
        /// </summary>
        public IEnumerable<string> RedListCategories { get; set; }
    }
}