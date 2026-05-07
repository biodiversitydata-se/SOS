using SOS.Lib.Models.Interfaces;
using System.Text.Json.Serialization;

namespace SOS.Lib.Models.Verbatim.VirtualHerbarium;

/// <summary>
///     Verbatim from Virtual Herbarium (New NDJSON format)
/// </summary>
public class VirtualHerbariumObservationVerbatim : IEntity<int>
{
    /// <summary>
    ///     Unique id (generated)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     Institution Code
    /// </summary>
    public string? InstCode { get; set; }

    /// <summary>
    ///     Accession Number
    /// </summary>
    public string? AccessionNo { get; set; }

    /// <summary>
    ///     Day of collection
    /// </summary>
    public string? Dag { get; set; }

    /// <summary>
    ///     Month of collection
    /// </summary>
    public string? Manad { get; set; }

    /// <summary>
    ///     Year of collection
    /// </summary>
    public string? Ar { get; set; }

    /// <summary>
    ///     Date collected (formatted string, e.g., "1944-06-13")
    /// </summary>
    public string? Datum { get; set; }

    /// <summary>
    ///     Scientific name of the taxon
    /// </summary>
    public string? TaxaName { get; set; }

    /// <summary>
    ///     Genus name
    /// </summary>
    public string? Genus { get; set; }

    /// <summary>
    ///     Species epithet
    /// </summary>
    public string? Species { get; set; }

    /// <summary>
    ///     Infraspecific rank (subspecies, variety, etc.)
    /// </summary>
    public string? InomArtsRang { get; set; }

    /// <summary>
    ///     Infraspecific epithet
    /// </summary>
    public string? InomArtsEpitet { get; set; }

    /// <summary>
    ///     Higher taxa
    /// </summary>
    public string? HigherTaxa { get; set; }

    /// <summary>
    ///     Collector name(s)
    /// </summary>
    public string? Collector { get; set; }

    /// <summary>
    ///     Collector number
    /// </summary>
    public string? CollectorNumber { get; set; }

    /// <summary>
    ///     Comments
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    ///     Continent
    /// </summary>
    public string? Continent { get; set; }

    /// <summary>
    ///     Country
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    ///     Province (Swedish landskap)
    /// </summary>
    public string? Province { get; set; }

    /// <summary>
    ///     District (Swedish socken/kommun)
    /// </summary>
    public string? District { get; set; }

    /// <summary>
    ///     Location/Locality name
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    ///     Habitat description
    /// </summary>
    public string? Habitat { get; set; }

    /// <summary>
    ///     Altitude
    /// </summary>
    public string? Altitude { get; set; }

    /// <summary>
    ///     Original name
    /// </summary>
    public string? OriginalNamn { get; set; }

    /// <summary>
    ///     Image reference
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    ///     Original text
    /// </summary>
    public string? OriginalText { get; set; }

    /// <summary>
    ///     Notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    ///     Rikets N coordinate (Swedish grid)
    /// </summary>
    public string? RiketsN { get; set; }

    /// <summary>
    ///     Rikets O coordinate (Swedish grid)
    /// </summary>
    public string? RiketsO { get; set; }

    /// <summary>
    ///     WGS84 Latitude
    /// </summary>
    public string? Wgs84_Lat { get; set; }

    /// <summary>
    ///     WGS84 Longitude
    /// </summary>
    public string? Wgs84_Lng { get; set; }

    /// <summary>
    ///     Rubin code
    /// </summary>
    public string? Rubin { get; set; }

    /// <summary>
    ///     Type status
    /// </summary>
    public string? TypeStatus { get; set; }

    /// <summary>
    ///     Basionym
    /// </summary>
    public string? Basionym { get; set; }

    /// <summary>
    ///     Type author
    /// </summary>
    public string? TypeAuktor { get; set; }

    /// <summary>
    ///     History/provenance
    /// </summary>
    public string? History { get; set; }

    /// <summary>
    ///     Hybrid name
    /// </summary>
    public string? HybridName { get; set; }
}
