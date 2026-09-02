namespace SOS.Api.Contracts.Metadata.Responses;
public class ProviderInfoDto
{

    public int? DataProviderId { get; set; }

    public string DataProviderIdentifier { get; set; }

    /// <summary>
    /// Number of items harvested
    /// </summary>
    public int? HarvestCount { get; set; }

    /// <summary>
    /// Harvest end date and time
    /// </summary>
    public System.DateTimeOffset? HarvestEnd { get; set; }

    /// <summary>
    /// Harvest note
    /// </summary>
    public string HarvestNotes { get; set; }

    /// <summary>
    /// Harvest start date and time
    /// </summary>
    public System.DateTimeOffset? HarvestStart { get; set; }

    /// <summary>
    /// Status of harvest
    /// </summary>
    public string HarvestStatus { get; set; }

    /// <summary>
    /// Last incremental process count
    /// </summary>
    public int? LatestIncrementalPublicCount { get; set; }

    /// <summary>
    /// Last incremental process count
    /// </summary>
    public int? LatestIncrementalProtectedCount { get; set; }

    /// <summary>
    /// Last incremental process end
    /// </summary>
    public System.DateTimeOffset? LatestIncrementalEnd { get; set; }

    /// <summary>
    /// Last incremental process status
    /// </summary>
    public string LatestIncrementalStatus { get; set; }

    /// <summary>
    /// Last incremental process start
    /// </summary>
    public System.DateTimeOffset? LatestIncrementalStart { get; set; }

    /// <summary>
    /// Number of items processed
    /// </summary>
    public int? PublicProcessCount { get; set; }

    /// <summary>
    /// Number of items processed
    /// </summary>
    public int? ProtectedProcessCount { get; set; }

    /// <summary>
    /// Process end date and time
    /// </summary>
    public System.DateTimeOffset? ProcessEnd { get; set; }

    /// <summary>
    /// Process start date and time
    /// </summary>
    public System.DateTimeOffset ProcessStart { get; set; }

    /// <summary>
    /// Status of processing
    /// </summary>
    public string ProcessStatus { get; set; }

    /// <summary>
    /// Id of data provider
    /// </summary>
    public string DataProviderType { get; set; }

}


