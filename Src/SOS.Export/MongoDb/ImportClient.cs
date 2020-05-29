using MongoDB.Driver;
using SOS.Export.MongoDb.Interfaces;

namespace SOS.Export.MongoDb
{
    /// <summary>
    ///     Export client
    /// </summary>
    public class ExportClient : MongoClient, IExportClient
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
        public ExportClient(MongoClientSettings settings, string dataBaseName, int batchSize) : base(settings)
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