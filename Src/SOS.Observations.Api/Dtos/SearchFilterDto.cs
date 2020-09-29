using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Observations.Api.Dtos
{
    /// <summary>
    /// Search filter.
    /// </summary>
    public class SearchFilterDto
    {
        /// <summary>
        ///     Only get data from these providers.
        /// </summary>
        public IEnumerable<int> DataProviderIds { get; set; }

        /// <summary>
        ///     This parameter allows you to create a dynamic view of the collection, or more precisely,
        ///     to decide what fields should or should not be returned, using a projection. Put another way,
        ///     your projection is a conditional query where you dictates which fields should be returned by the API.
        ///     Omit this parameter and you will receive the complete collection of fields.
        /// </summary>
        public IEnumerable<string> OutputFields { get; set; }

        /// <summary>
        /// Date filter.
        /// </summary>
        public DateFilterDto Date { get; set; }

        /// <summary>
        /// Taxon filter.
        /// </summary>
        public TaxonFilterDto Taxon { get; set; }

        /// <summary>
        /// Region filter.
        /// </summary>
        public IEnumerable<int> RegionIds { get; set; }

        /// <summary>
        /// Geometry filter 
        /// </summary>
        public GeometryFilterDto Geometry { get; set; }

        /// <summary>
        /// True to return only validated sightings.
        /// </summary>
        public bool? OnlyValidated { get; set; }

        /// <summary>
        /// True to return only positive sightings, false to return negative sightings, null to return both positive and
        /// negative sightings.
        /// A negative observation is an observation that was expected to be found but wasn't.
        /// </summary>
        public bool? PositiveSightings { get; set; }

        /// <summary>
        ///  Field mapping translation culture code.
        ///  Available values.
        ///  sv-SE (Swedish)
        ///  en-GB (English)
        /// </summary>
        public string FieldTranslationCultureCode { get; set; }
    }
}