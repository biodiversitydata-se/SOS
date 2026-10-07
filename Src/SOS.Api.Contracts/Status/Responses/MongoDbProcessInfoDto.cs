namespace SOS.Api.Contracts.Status.Responses;
/// <summary>Observation processing information read from MongoDB.</summary>
public class MongoDbProcessInfoDto
{
    /// <summary>Processing and harvesting information for one data provider.</summary>
    public class Provider
    {
        /// <summary>The numeric data provider identifier.</summary>
        public int? DataProviderId { get; set; }

        /// <summary>The data provider's string identifier.</summary>
        public string? DataProviderIdentifier { get; set; }

        /// <summary>The time processing finished for this provider.</summary>
        public DateTime? ProcessEnd { get; set; }

        /// <summary>The time processing started for this provider.</summary>
        public DateTime? ProcessStart { get; set; }

        /// <summary>The processing status for this provider.</summary>
        public string? ProcessStatus { get; set; }

        /// <summary>The number of provider observations that failed processing.</summary>
        public int? ProcessFailCount { get; set; }

        /// <summary>The number of public provider observations processed.</summary>
        public int? PublicProcessCount { get; set; }

        /// <summary>The number of protected provider observations processed.</summary>
        public int? ProtectedProcessCount { get; set; }

        /// <summary>The time harvesting finished for this provider.</summary>
        public DateTime? HarvestEnd { get; set; }

        /// <summary>The time harvesting started for this provider.</summary>
        public DateTime? HarvestStart { get; set; }

        /// <summary>Notes from harvesting this provider's data.</summary>
        public string? HarvestNotes { get; set; }

        /// <summary>The harvesting status for this provider.</summary>
        public string? HarvestStatus { get; set; }

        /// <summary>The number of records harvested from this provider.</summary>
        public int? HarvestCount { get; set; }

        /// <summary>The end time of the provider's latest incremental processing run.</summary>
        public DateTime? LatestIncrementalEnd { get; set; }

        /// <summary>The start time of the provider's latest incremental processing run.</summary>
        public DateTime? LatestIncrementalStart { get; set; }

        /// <summary>The status code of the provider's latest incremental processing run.</summary>
        public int? LatestIncrementalStatus { get; set; }

        /// <summary>The number of public observations processed in the provider's latest incremental run.</summary>
        public int? LatestIncrementalPublicCount { get; set; }

        /// <summary>The number of protected observations processed in the provider's latest incremental run.</summary>
        public int? LatestIncrementalProtectedCount { get; set; }

        /// <summary>The provider type associated with the processing information.</summary>
        public string? DataProviderType { get; set; }
    }

    /// <summary>The identifier of the processing run.</summary>
    public string? Id { get; set; }

    /// <summary>The total number of observations that failed processing.</summary>
    public int ProcessFailCount { get; set; }

    /// <summary>The total number of public observations processed.</summary>
    public int PublicCount { get; set; }

    /// <summary>The total number of protected observations processed.</summary>
    public int ProtectedCount { get; set; }

    /// <summary>The time the processing run finished.</summary>
    public DateTime End { get; set; }

    /// <summary>The time the processing run started.</summary>
    public DateTime Start { get; set; }

    /// <summary>The status of the processing run.</summary>
    public string? Status { get; set; }

    /// <summary>Harvesting and processing information for each data provider.</summary>
    public IEnumerable<Provider>? ProvidersInfo { get; set; }
}


