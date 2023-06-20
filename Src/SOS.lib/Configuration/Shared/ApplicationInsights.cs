namespace SOS.Lib.Configuration.Shared
{
    /// <summary>
    /// Configuration for application insights
    /// </summary>
    public class ApplicationInsights
    {
        /// <summary>
        /// Connection string
        /// </summary>
        public string ConnectionString => $"InstrumentationKey={InstrumentationKey};IngestionEndpoint=https://northeurope-3.in.applicationinsights.azure.com/;LiveEndpoint=https://northeurope.livediagnostics.monitor.azure.com/";
        
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