using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///     Base class for cosmos db repositories
    /// </summary>
    public class ProcessRepositoryBase<TEntity> : IProcessRepositoryBase<TEntity>
    {
        private readonly IProcessClient _client;
        private readonly string _collectionNameConfiguration = typeof(ProcessedConfiguration).Name;
        private readonly bool _toggleable;

        /// <summary>
        ///     Disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        ///     Mongo db
        /// </summary>
        private IMongoDatabase _database;

        /// <summary>
        ///     Get configuration object
        /// </summary>
        /// <returns></returns>
        private ProcessedConfiguration GetConfiguration()
        {
            try
            {
                return MongoCollectionConfiguration
                    .Find(Builders<ProcessedConfiguration>.Filter.Empty)
                    .FirstOrDefault();
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());

                return default;
            }
        }

        /// <summary>
        ///     Make sure configuration collection exists
        /// </summary>
        private void InitializeConfiguration()
        {
            if (_database == null)
            {
                return;
            }

            //filter by collection name
            var exists = _database
                .ListCollectionNames(new ListCollectionNamesOptions
                {
                    Filter = new BsonDocument("name", _collectionNameConfiguration)
                })
                .Any();

            //check for existence
            if (!exists)
            {
                // Create the collection
                _database.CreateCollection(_collectionNameConfiguration);

                MongoCollectionConfiguration.InsertOne(new ProcessedConfiguration
                {
                    ActiveInstance = 1
                });
            }
        }

        /// <summary>
        ///     Configuration collection
        /// </summary>
        private IMongoCollection<ProcessedConfiguration> MongoCollectionConfiguration => _database
            .GetCollection<ProcessedConfiguration>(_collectionNameConfiguration)
            .WithWriteConcern(new WriteConcern(1, journal: true));

        /// <summary>
        ///     Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            _disposed = true;
        }

        /// <summary>
        ///     Logger
        /// </summary>
        protected readonly ILogger<ProcessRepositoryBase<TEntity>> Logger;

        /// <summary>
        /// Get name of instance
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        protected string GetInstanceName(byte instance) =>  _toggleable
                ? $"{typeof(TEntity).Name.UntilNonAlfanumeric()}-{instance}"
                : $"{typeof(TEntity).Name.UntilNonAlfanumeric()}";
        
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="toggleable"></param>
        /// <param name="logger"></param>
        public ProcessRepositoryBase(
            IProcessClient client,
            bool toggleable,
            ILogger<ProcessRepositoryBase<TEntity>> logger
        )
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _toggleable = toggleable;
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _database = _client.GetDatabase();
            BatchSize = _client.WriteBatchSize;
            // Init config
            InitializeConfiguration();
        }

        /// <inheritdoc />
        public int BatchSize { get; }

        /// <summary>
        ///     Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public JobRunModes Mode { get; set; }

        /// <inheritdoc />
        public byte ActiveInstance => GetConfiguration()?.ActiveInstance ?? 1;

        /// <inheritdoc />
        public byte InActiveInstance => (byte) (ActiveInstance == 0 ? 1 : 0);

        /// <inheritdoc />
        public string ActiveInstanceName => GetInstanceName(ActiveInstance);

        /// <inheritdoc />
        public string InactiveInstanceName => GetInstanceName(InActiveInstance);

        /// <inheritdoc />
        public string CurrentInstanceName=> Mode == JobRunModes.IncrementalActiveInstance ? ActiveInstanceName : InactiveInstanceName;

        /// <inheritdoc />
        public async Task<bool> SetActiveInstanceAsync(byte instance)
        {
            try
            {
                var config = GetConfiguration();

                config.ActiveInstance = instance;

                var updateResult = await MongoCollectionConfiguration.ReplaceOneAsync(
                    x => x.Id.Equals(config.Id),
                    config,
                    new ReplaceOptions { IsUpsert = true });

                return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());

                return default;
            }
        }
    }
}