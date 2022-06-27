namespace SOS.Lib.Configuration.ObservationApi
{
    /// <summary>
    /// Configuration for health check
    /// </summary>
    public class HealthCheckConfiguration
    {
        /// <summary>
        /// Approximately number of observations expected to be in protected index
        /// </summary>
        public int ProtectedObservationCount { get; set; }

        /// <summary>
        /// Approximately number of observations expected to be in public index
        /// </summary>
        public int PublicObservationCount { get; set; }

        /// <summary>
        /// Minimum local disk storage available (GB)
        /// </summary>
        public double MinimumLocalDiskStorage { get; set; }

        /// <summary>
        /// Azure API URL.
        /// </summary>
        public string AzureApiUrl { get; set; }

        /// <summary>
        /// Elasticsearch Proxy URL.
        /// </summary>
        public string ElasticsearchProxyUrl { get; set; }

        /// <summary>
        /// Azure API Subscription key.
        /// </summary>
        public string AzureSubscriptionKey { get; set; }
    }
}