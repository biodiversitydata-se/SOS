namespace SOS.Lib.Configuration.Shared
{
    /// <summary>
    ///     Hangfire Db configuration
    /// </summary>
    public class HangfireDbConfiguration : MongoDbConfiguration
    {
        /// <summary>
        ///     The number of days a successful or deleted job will be kept in db.
        /// </summary>
        public int JobExpirationDays { get; set; }
    }
}