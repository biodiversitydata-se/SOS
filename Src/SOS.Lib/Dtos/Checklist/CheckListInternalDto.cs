namespace SOS.Lib.Dtos.Checklist;

public class ChecklistInternalDto : ChecklistDto
{
    /// <summary>
    /// Values used internal in Artportalen
    /// </summary>
    public ApInternalDto ArtportalenInternal { get; set; }
}