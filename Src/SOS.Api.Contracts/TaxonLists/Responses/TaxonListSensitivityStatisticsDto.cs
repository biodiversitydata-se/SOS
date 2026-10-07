using System.Collections.Generic;

namespace SOS.Api.Contracts.TaxonLists.Responses;

/// <summary>
/// Taxa and counts grouped by sensitivity category.
/// </summary>
public class TaxonListSensitivityStatisticsDto
{
    /// <summary>Taxon identity and scientific and Swedish names in sensitivity statistics.</summary>
    public class TaxonInfo
    {
        /// <summary>The Dyntaxa identifier of the taxon.</summary>
        public int Id { get; set; }
        /// <summary>The taxon's scientific name.</summary>
        public string ScientificName { get; set; }
        /// <summary>The taxon's Swedish name.</summary>
        public string SwedishName { get; set; }
    }

    /// <summary>The number of taxa in sensitivity category 3.</summary>
    public int Category3Count { get; set; }
    /// <summary>The number of taxa in sensitivity category 4.</summary>
    public int Category4Count { get; set; }
    /// <summary>The number of taxa in sensitivity category 5.</summary>
    public int Category5Count { get; set; }
    /// <summary>The taxa in sensitivity category 3.</summary>
    public List<TaxonInfo> Category3Taxa { get; set; }
    /// <summary>The taxa in sensitivity category 4.</summary>
    public List<TaxonInfo> Category4Taxa { get; set; }
    /// <summary>The taxa in sensitivity category 5.</summary>
    public List<TaxonInfo> Category5Taxa { get; set; }
}
