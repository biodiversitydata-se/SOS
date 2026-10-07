using System.Collections.Generic;

namespace SOS.Api.Contracts.Aggregations.Responses;

/// <summary>
/// A page of map grid cells with taxon observation counts.
/// </summary>
public class GeoGridTileTaxonPageResultDto
{
    /// <summary>Whether additional grid-cell and taxon results are available.</summary>
    public bool HasMorePages { get; set; }

    /// <summary>
    /// The GeoTile page key.
    /// </summary>
    public string NextGeoTilePage { get; set; }

    /// <summary>
    /// The TaxonId page key.
    /// </summary>
    public int? NextTaxonIdPage { get; set; }
    /// <summary>The grid cells and taxon counts returned in this page.</summary>
    public IEnumerable<GeoGridTileTaxaCellDto> GridCells { get; set; }
}
