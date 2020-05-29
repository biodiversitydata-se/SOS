using MongoDB.Driver;
using SOS.Process.Database.Interfaces;

namespace SOS.Process.Database
{
    public class VerbatimClient : MongoClient, IVerbatimClient
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
        public VerbatimClient(MongoClientSettings settings, string dataBaseName, int batchSize) : base(settings)
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