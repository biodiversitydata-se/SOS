namespace SOS.Lib.Configuration.ObservationApi
{
    /// <summary>
    /// Configuration for application insights
    /// </summary>
    public class ApplicationInsights
    {
        /// <summary>
        /// Key to application insights
        /// </summary>
        public string InstrumentationKey { get; set; }

        /// <summary>
        /// Log request body 
        /// </summary>
        public bool EnableRequestBodyLogging { get; set; }

        /// <summary>
        /// Log search response document count
        /// </summary>
        public bool EnableSearchResponseCountLogging { get; set; }
    }
}