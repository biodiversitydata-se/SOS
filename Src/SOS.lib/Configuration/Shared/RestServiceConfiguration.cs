namespace SOS.Lib.Configuration.Shared
{
    /// <summary>
    ///     Configuration parameters for a REST service
    /// </summary>
    public class RestServiceConfiguration
    {
        /// <summary>
        ///     Address for the service
        /// </summary>
        public string BaseAddress { get; set; }

        /// <summary>
        ///     Content type of the acceptheader
        /// </summary>
        public string AcceptHeaderContentType { get; set; }
    }
}