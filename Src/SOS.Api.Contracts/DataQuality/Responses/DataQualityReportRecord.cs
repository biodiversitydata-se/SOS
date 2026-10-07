using System.Collections.Generic;

namespace SOS.Api.Contracts.DataQuality.Responses;

/// <summary>
/// Data quality report record
/// </summary>
public class DataQualityReportRecord
{
    /// <summary>
    /// Constructor
    /// </summary>
    public DataQualityReportRecord()
    {

    }

    /// <summary>
    /// Observation rnd date
    /// </summary>
    public string EndDate { get; set; } = null!;

    /// <summary>
    /// Observation locality
    /// </summary>
    public string Locality { get; set; } = null!;

    /// <summary>
    /// Duplicate observations
    /// </summary>
    public IEnumerable<DataQualityReportObservation> Observations { get; set; } = null!;

    /// <summary>
    /// Observation start date
    /// </summary>
    public string StartDate { get; set; } = null!;

    /// <summary>
    /// Id of observed taxon
    /// </summary>
    public string TaxonId { get; set; } = null!;

    /// <summary>
    /// Scientific name of taxon
    /// </summary>
    public string TaxonScientificName { get; set; } = null!;

    /// <summary>
    /// Unique key
    /// </summary>
    public string UniqueKey { get; set; } = null!;
}
