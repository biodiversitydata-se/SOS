using System;

namespace SOS.Shared.Api.Dtos;

/// <summary>
/// Taxon observation count.
/// </summary>
public class TaxonObservationCountDto
{
    /// <summary>
    /// The Taxon Id.
    /// </summary>
    public int TaxonId { get; set; }

    /// <summary>
    /// Observation count.
    /// </summary>
    public int ObservationCount { get; set; }

    /// <summary>
    /// Province count.
    /// </summary>
    public int ProvinceCount { get; set; }
}