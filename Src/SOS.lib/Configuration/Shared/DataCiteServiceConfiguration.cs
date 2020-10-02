namespace SOS.Lib.Configuration.Shared
{
    public class DataCiteServiceConfiguration : RestServiceConfiguration
    {
        /// <summary>
        /// Client id in DataCite
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Datacite prefix
        /// </summary>
        public string DoiPrefix { get; set; }

        /// <summary>
        /// Url to DOI information and file
        /// </summary>
        public string DoiUrl { get; set; }

        /// <summary>
        /// Basic authentication password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Basic authentication user name
        /// </summary>
        public string UserName { get; set; }
    }
}