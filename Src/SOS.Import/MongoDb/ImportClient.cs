using MongoDB.Driver;
using SOS.Import.MongoDb.Interfaces;

namespace SOS.Import.MongoDb
{
    public class ImportClient : MongoClient, IImportClient
    {
        /// <summary>
        ///     Name of database
        /// </summary>
        private readonly string _dataBaseName;


        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="dataBaseName"></param>
        /// <param name="batchSize"></param>
        public ImportClient(MongoClientSettings settings, string dataBaseName, int batchSize) : base(settings)
        {
            _dataBaseName = dataBaseName;
            BatchSize = batchSize;
        }

        /// <inheritdoc />
        public int BatchSize { get; }

        public IMongoDatabase GetDatabase()
        {
            return base.GetDatabase(_dataBaseName);
        }
    }
}