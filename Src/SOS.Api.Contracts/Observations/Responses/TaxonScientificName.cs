namespace SOS.Api.Contracts.Observations.Responses;

public sealed class TaxonScientificName
{
    public string? Author { get; set; }
    public string? Name { get; set; }
    public bool IsPreferredName { get; set; }
    public bool ValidForSighting { get; set; }
}
