namespace SOS.Lib.Models.ArtportalenApiService;

/// <summary>
/// Represents a single taxon hierarchy relation from Artportalen API
/// </summary>
public class TaxonHierarchyItem
{
    /// <summary>
    /// Gets or sets the parent taxon ID
    /// </summary>
    public int ParentId { get; set; }

    /// <summary>
    /// Gets or sets the child taxon ID
    /// </summary>
    public int ChildId { get; set; }

    /// <summary>
    /// Gets or sets the relation type
    /// </summary>
    public int Relation { get; set; }
}
