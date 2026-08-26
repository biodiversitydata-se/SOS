using System.Collections.Generic;

namespace SOS.Lib.Models.ArtportalenApiService;

/// <summary>
/// Represents the paginated response for taxon hierarchy from Artportalen API
/// </summary>
public class TaxonHierarchyResponse
{
    /// <summary>
    /// Gets or sets the pager information
    /// </summary>
    public Pager Pager { get; set; }

    /// <summary>
    /// Gets or sets the taxon hierarchy data
    /// </summary>
    public List<TaxonHierarchyItem> Data { get; set; }
}

/// <summary>
/// Represents pagination information
/// </summary>
public class Pager
{
    /// <summary>
    /// Gets or sets the offset
    /// </summary>
    public int Offset { get; set; }

    /// <summary>
    /// Gets or sets the limit
    /// </summary>
    public int Limit { get; set; }

    /// <summary>
    /// Gets or sets the total count
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the sort field
    /// </summary>
    public string SortField { get; set; }

    /// <summary>
    /// Gets or sets the sort order
    /// </summary>
    public string SortOrder { get; set; }
}
