using MongoDB.Driver;

namespace SOS.Observations.Api.Database.Interfaces
{
    /// <summary>
    /// Process client.
    /// </summary>
    public interface IProcessClient : IMongoClient
    {
        /// <summary>
        /// Batch size.
        /// </summary>
        int BatchSize { get; }

        /// <summary>
        /// Get database
        /// </summary>
        /// <returns></returns>
        IMongoDatabase GetDatabase();
    }
}
