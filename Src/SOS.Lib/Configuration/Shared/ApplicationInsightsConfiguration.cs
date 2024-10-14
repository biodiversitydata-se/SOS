namespace SOS.Lib.Configuration.Shared
{
    public class ApplicationInsightsConfiguration
    {
        /// <summary>
        ///     Address for the service
        /// </summary>
        public string BaseAddress { get; set; }

        /// <summary>
        ///     Content type of the acceptheader
        /// </summary>
        public string AcceptHeaderContentType { get; set; }

        public string ApiKey { get; set; }
        public string ApplicationId { get; set; }
        public string InstrumentationKey { get; set; }
    }
}
