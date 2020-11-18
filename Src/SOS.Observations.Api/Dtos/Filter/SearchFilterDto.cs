using System;
using System.Collections.Generic;

namespace SOS.Observations.Api.Dtos.Filter
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
        /// Area filter
        /// </summary>
        public IEnumerable<AreaFilterDto> Areas { get; set; }

        /// <summary>
        /// Area filter.
        /// </summary>
        [Obsolete("To be removed. Use Areas instead")]
        public IEnumerable<int> AreaIds { get; set; }

        /// <summary>
        /// Geometry filter 
        /// </summary>
        public GeometryFilterDto Geometry { get; set; }

        /// <summary>
        /// Set to true to return only validated sightings.
        /// </summary>
        public bool? OnlyValidated { get; set; }

        /// <summary>
        /// This property indicates whether to search for present observations and/or absent observations.
        /// If no value is set, this will be set to include only present observations.
        /// </summary>
        public OccurrenceStatusFilterValuesDto? OccurrenceStatus { get; set; }

        /// <summary>
        ///  Translation culture code for fields that are using a vocabulary.
        ///  Available values.
        ///  sv-SE (Swedish)
        ///  en-GB (English)
        /// </summary>
        public string TranslationCultureCode { get; set; }
    }
}