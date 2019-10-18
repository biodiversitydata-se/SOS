namespace SOS.Hangfire.UI.Configuration
{
    /// <summary>
    /// Application settings
    /// </summary>
    public class ApplicationSettings
    {
        /// <summary>
        /// Mongo db connection settings
        /// </summary>
        public MongoDbConfiguration MongoDbRepository { get; set; }
    }
}