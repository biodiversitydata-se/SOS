using MongoDB.Driver;

namespace SOS.Process.Database.Interfaces
{
    public interface IProcessClient : IMongoClient
    {
        int BatchSize { get; }

        /// <summary>
        /// Get database
        /// </summary>
        /// <returns></returns>
        IMongoDatabase GetDatabase();

        /// <summary>
        /// Initialize client
        /// </summary>
        /// <param name="databaseName"></param>
        void Initialize(string databaseName);
    }
}
