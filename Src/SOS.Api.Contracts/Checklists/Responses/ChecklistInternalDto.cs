using SOS.Api.Contracts.Checklists.Responses;
namespace SOS.Api.Contracts.Checklists.Responses;

/// <summary>A checklist with additional Artportalen identifiers.</summary>
public class ChecklistInternalDto : ChecklistDto
{
    /// <summary>
    /// Values used internal in Artportalen
    /// </summary>
    public ApInternalDto ArtportalenInternal { get; set; }
}
