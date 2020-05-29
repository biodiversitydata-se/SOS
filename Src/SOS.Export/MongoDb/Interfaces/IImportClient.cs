using MongoDB.Driver;

namespace SOS.Export.MongoDb.Interfaces
{
    public interface IExportClient : IMongoClient
    {
        int BatchSize { get; }

        /// <summary>
        ///     Get database
        /// </summary>
        /// <returns></returns>
        IMongoDatabase GetDatabase();
    }
}