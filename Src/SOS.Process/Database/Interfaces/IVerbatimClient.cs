using MongoDB.Driver;

namespace SOS.Process.Database.Interfaces
{
    /// <summary>
    ///     Verbatim client
    /// </summary>
    public interface IVerbatimClient : IMongoClient
    {
        /// <summary>
        ///     Size of batch
        /// </summary>
        int BatchSize { get; }

        /// <summary>
        ///     Get database
        /// </summary>
        /// <returns></returns>
        IMongoDatabase GetDatabase();
    }
}