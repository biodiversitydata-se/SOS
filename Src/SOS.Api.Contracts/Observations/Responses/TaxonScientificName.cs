namespace SOS.Api.Contracts.Observations.Responses;

/// <summary>A taxon's scientific name, authorship and usage flags.</summary>
public sealed class TaxonScientificName
{
    /// <summary>The authorship associated with the scientific name.</summary>
    public string? Author { get; set; }
    /// <summary>The scientific name.</summary>
    public string? Name { get; set; }
    /// <summary>Whether this is the taxon's preferred scientific name.</summary>
    public bool IsPreferredName { get; set; }
    /// <summary>Whether this scientific name may be used to report an observation.</summary>
    public bool ValidForSighting { get; set; }
}
