using MongoDB.Driver;

namespace SOS.Import.MongoDb.Interfaces
{
    public interface IImportClient : IMongoClient
    {
        int BatchSize { get; }

        /// <summary>
        /// Get database
        /// </summary>
        /// <returns></returns>
        IMongoDatabase GetDatabase();
    }
}
