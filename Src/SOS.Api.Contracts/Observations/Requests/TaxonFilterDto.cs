namespace SOS.Api.Contracts.Observations.Requests;
/// <summary>
/// Taxon filter.
/// </summary>
public class TaxonFilterDto
{

    /// <summary>
    /// If true, also include the underlying hierarchical taxa in search.
    /// <br/>E.g. If ids=[4000104](Aves) and includeUnderlyingTaxa=true, then you search for all birds.
    /// </summary>
    public bool? IncludeUnderlyingTaxa { get; set; }

    /// <summary>
    /// Dyntaxa taxon id's to match.
    /// </summary>
    public ICollection<int>? Ids { get; set; }

    /// <summary>
    /// Add (merge) or filter taxa by using taxon lists.
    /// </summary>
    public ICollection<int>? TaxonListIds { get; set; }

    /// <summary>
    /// Filters taxa by whether they are classified as invasive in Sweden.
    /// </summary>
    public bool? IsInvasiveInSweden { get; set; }

    /// <summary>
    /// Redlist categories to match.
    /// <br/>Possible values are: "DD", "EX", "RE", "CR", "EN", "VU", "NT", "LC", "NA", "NE"
    /// </summary>
    public ICollection<string>? RedListCategories { get; set; }

    /// <summary>
    /// Taxon categories to match.
    /// </summary>
    public ICollection<int>? TaxonCategories { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public TaxonListOperatorDto TaxonListOperator { get; set; }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public TaxonomyTypeDto TaxonomyType { get; set; }
}


