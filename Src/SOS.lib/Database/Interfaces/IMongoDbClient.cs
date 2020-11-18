using MongoDB.Driver;

namespace SOS.Lib.Database.Interfaces
{
    /// <summary>
    /// Mongo db client
    /// </summary>
    public interface IMongoDbClient : IMongoClient
    {
        /// <summary>
        /// Read batch size
        /// </summary>
        public int ReadBatchSize { get; }

        /// <summary>
        /// Write batch size
        /// </summary>
        public int WriteBatchSize { get; }

        /// <summary>
        ///     Get database
        /// </summary>
        /// <returns></returns>
        IMongoDatabase GetDatabase();
    }
}