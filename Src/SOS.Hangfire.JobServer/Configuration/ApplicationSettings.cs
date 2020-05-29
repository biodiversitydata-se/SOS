using SOS.Lib.Configuration.Shared;

namespace SOS.Hangfire.JobServer.Configuration
{
    /// <summary>
    ///     Application settings
    /// </summary>
    public class ApplicationSettings
    {
        /// <summary>
        ///     Mongo db connection settings
        /// </summary>
        public MongoDbConfiguration HangfireDbConfiguration { get; set; }
    }
}