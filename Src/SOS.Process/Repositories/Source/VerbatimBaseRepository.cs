﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Repositories.Source
{
    /// <summary>
    ///     Base class for verbatim repositories
    /// </summary>
    public class VerbatimBaseRepository<TEntity, TKey> : IVerbatimBaseRepository<TEntity, TKey>
        where TEntity : IEntity<TKey>
    {
        private readonly string _collectionName;

        protected readonly IVerbatimClient Client;

        /// <summary>
        ///     Mongo db
        /// </summary>
        protected readonly IMongoDatabase Database;

        /// <summary>
        ///     Logger
        /// </summary>
        protected readonly ILogger<VerbatimBaseRepository<TEntity, TKey>> Logger;

        /// <summary>
        ///     Disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        protected VerbatimBaseRepository(
            IVerbatimClient client,
            ILogger<VerbatimBaseRepository<TEntity, TKey>> logger
        )
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            Client = client;
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Database = client.GetDatabase();
            _collectionName = typeof(TEntity).Name;
        }

        /// <summary>
        ///     Get client
        /// </summary>
        /// <returns></returns>
        protected IMongoCollection<TEntity> MongoCollection => GetMongoCollection(Mode == JobRunModes.Full ? _collectionName : $"{_collectionName}_incremental");

        /// <inheritdoc />
        public async Task<bool> CheckIfCollectionExistsAsync()
        {
            return await CheckIfCollectionExistsAsync(_collectionName);
        }

        /// <inheritdoc />
        public async Task<bool> CheckIfCollectionExistsAsync(string collectionName)
        {
            //filter by collection name
            var exists = await (await Database
                    .ListCollectionNamesAsync(new ListCollectionNamesOptions
                    {
                        Filter = new BsonDocument("name", collectionName)
                    }))
                .AnyAsync();

            return exists;
        }

        /// <inheritdoc />
        public async Task<IAsyncCursor<TEntity>> GetAllByCursorAsync()
        {
            try
            {
                return await GetAllByCursorAsync(MongoCollection);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error when executing GetAllByCursorAsync()");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IAsyncCursor<TEntity>> GetAllByCursorAsync(IMongoCollection<TEntity> mongoCollection,
            bool noCursorTimeout = false)
        {
            return await mongoCollection.FindAsync(FilterDefinition<TEntity>.Empty,
                new FindOptions<TEntity, TEntity> {NoCursorTimeout = noCursorTimeout});
        }

        /// <inheritdoc />
        public async Task<List<TEntity>> GetAllAsync()
        {
            return await GetAllAsync(MongoCollection);
        }

        /// <inheritdoc />
        public async Task<List<TEntity>> GetAllAsync(IMongoCollection<TEntity> mongoCollection)
        {
            return await mongoCollection.AsQueryable().ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> GetBatchAsync(TKey startId, TKey endId)
        {
            return await GetBatchAsync(startId, endId, MongoCollection);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> GetBatchAsync(TKey startId, TKey endId,
            IMongoCollection<TEntity> mongoCollection)
        {
            try
            {
                var filters = new List<FilterDefinition<TEntity>>
                {
                    Builders<TEntity>.Filter.Gte(d => d.Id, startId),
                    Builders<TEntity>.Filter.Lte(d => d.Id, endId)
                };

                var res = await mongoCollection
                    .Find(Builders<TEntity>.Filter.And(filters))
                    .ToListAsync();

                return res;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());

                return default;
            }
        }

        /// <inheritdoc />
        public async Task<Tuple<TKey, TKey>> GetIdSpanAsync()
        {
            return await GetIdSpanAsync(MongoCollection);
        }

        /// <inheritdoc />
        public async Task<Tuple<TKey, TKey>> GetIdSpanAsync(IMongoCollection<TEntity> mongoCollection)
        {
            try
            {
                var min = await mongoCollection
                    .Find(FilterDefinition<TEntity>.Empty)
                    .Project(d => d.Id)
                    .Sort(Builders<TEntity>.Sort.Ascending("_id"))
                    .Limit(1)
                    .FirstOrDefaultAsync();

                var max = await mongoCollection
                    .Find(FilterDefinition<TEntity>.Empty)
                    .Project(d => d.Id)
                    .Sort(Builders<TEntity>.Sort.Descending("_id"))
                    .Limit(1)
                    .FirstOrDefaultAsync();

                return new Tuple<TKey, TKey>(min, max);
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());

                return default;
            }
        }

        /// <inheritdoc />
        public JobRunModes Mode { get; set; }

        /// <summary>
        ///     Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected IMongoCollection<TEntity> GetMongoCollection(string collectionName)
        {
            return Database.GetCollection<TEntity>(collectionName);
        }

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
    }
}