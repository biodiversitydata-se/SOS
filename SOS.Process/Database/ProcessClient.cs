using MongoDB.Driver;

namespace SOS.Process.Database
{
    public class ProcessClient : MongoClient, Interfaces.IProcessClient
    {
        /// <inheritdoc />
        public int BatchSize { get; }

        /// <summary>
        /// Name of database
        /// </summary>
        private string _databaseName;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="batchSize"></param>
        public ProcessClient(MongoClientSettings settings, int batchSize) : base(settings)
        {
            BatchSize = batchSize;
        }

        public IMongoDatabase GetDatabase()
        {
            return base.GetDatabase(_databaseName);
        }

        /// <inheritdoc />
        public void Initialize(string databaseName)
        {
            _databaseName = databaseName;
        }
    }
}
