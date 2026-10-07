namespace SOS.Api.Contracts.Checklists.Responses;

// Artportalen internal values
/// <summary>
/// Artportalen-specific checklist identifiers.
/// </summary>
public class ApInternalDto
{
    /// <summary>
    /// Id of checklist
    /// </summary>
    public int ChecklistId { get; set; }

    /// <summary>
    /// Parent taxon id
    /// </summary>
    public int ParentTaxonId { get; set; }

    /// <summary>
    /// Id of controlling user
    /// </summary>
    public int UserId { get; set; }
}
