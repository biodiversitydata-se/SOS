﻿using SOS.Shared.Api.Dtos.Enum;

namespace SOS.Shared.Api.Dtos.Filter
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
        public IEnumerable<string>? RedListCategories { get; set; }

        /// <summary>
        /// Taxon categories to match.
        /// </summary>
        public IEnumerable<int>? TaxonCategories { get; set; }

        /// <summary>
        /// Operator to use when TaxonListIds is specified. The operators are Merge or Filter.
        /// </summary>
        public TaxonListOperatorDto TaxonListOperator { get; set; } = TaxonListOperatorDto.Merge;
    }
}