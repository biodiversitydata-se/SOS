namespace SOS.Api.Contracts.Lookups.Responses;
/// <summary>
/// Information about a data provider.
/// </summary>
public class DataProviderDto
{

    /// <summary>
    /// Id.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// A unique text identifer for the data provider.
    /// </summary>
    public string Identifier { get; set; }

    /// <summary>
    /// The name of the data provider.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Description of the data provider.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The organization name.
    /// </summary>
    public string Organization { get; set; }

    /// <summary>
    /// Paths that can be used to group and visualize a data provider as a tree in a GUI.
    /// </summary>
    public ICollection<string> Path { get; set; }

    /// <summary>
    /// URL to the data provider source.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Number of public observations.
    /// </summary>
    public int PublicObservations { get; set; }

    /// <summary>
    /// Number of protected observations.
    /// </summary>
    public int ProtectedObservations { get; set; }

    /// <summary>
    /// Latest harvest date.
    /// </summary>
    public System.DateTimeOffset? LatestHarvestDate { get; set; }

    /// <summary>
    /// Latest process date.
    /// </summary>
    public System.DateTimeOffset? LatestProcessDate { get; set; }

    /// <summary>
    /// Latest incremental harvest and process date. Used for data providers supporting incremental harvest.
    /// </summary>
    public System.DateTimeOffset? LatestIncrementalHarvestDate { get; set; }

    /// <summary>
    /// Date time from where next harvest can be run.
    /// </summary>
    public System.DateTimeOffset? NextHarvestFrom { get; set; }

    /// <summary>
    /// Note about harvest
    /// </summary>
    public string HarvestNotes { get; set; }

    /// <summary>
    /// Cron expression used to schedule harvest.
    /// </summary>
    public string HarvestSchedule { get; set; }

    /// <summary>
    /// Decides whether the data provider should be included in search when no data provider filter is set.
    /// </summary>
    public bool IncludeInSearchByDefault { get; set; }

}


