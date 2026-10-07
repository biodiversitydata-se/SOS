namespace SOS.Api.Contracts.TaxonLists.Responses;

/// <summary>
/// Information about a taxon list.
/// </summary>
public class TaxonListDto
{
    /// <summary>
    /// The Id of the taxon list.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The parent Id of the taxon list.
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// The Id in Taxon list service.
    /// </summary>
    public int TaxonListServiceId { get; set; }

    /// <summary>
    ///     The name of the taxon list.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The taxa in this taxon list.
    /// </summary>
    public ICollection<TaxonListTaxonInformationDto>? Taxa { get; set; }

}
