namespace SOS.Api.Contracts.DataQuality.Responses;

/// <summary>
/// Observation data 
/// </summary>
public class DataQualityReportObservation
{
    /// <summary>
    /// Id of data provider
    /// </summary>
    public string DataProviderId { get; set; } = null!;

    /// <summary>
    /// Id of observation
    /// </summary>
    public string OccurrenceId { get; set; } = null!;
}
