using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.NetTopologySuite.Serialization;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SOS.Lib.Repositories
{

    /// <summary>
    ///     Base class for cosmos db repositories
    /// </summary>
    public class RepositoryBase<TEntity, TKey> : IRepositoryBase<TEntity, TKey> where TEntity : IEntity<TKey>
    {
        private readonly string _collectionName;
        private readonly TimeSpan _serverTimeout = TimeSpan.FromMinutes(120); // Server-side timeout for operations
        private readonly TimeSpan _clientTimeout = TimeSpan.FromMinutes(120); // Client-side timeout for operations

        /// <summary>
        ///     Disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        private async Task<bool> AddBatchAsync(IEnumerable<TEntity> batch)
        {
            return await AddBatchAsync(batch, MongoCollection);
        }

        /// <summary>
        /// Add batch of items to mongodb
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="mongoCollection"></param>
        /// <param name="attempt"></param>
        /// <returns></returns>
        private async Task<bool> AddBatchAsync(IEnumerable<TEntity> batch, IMongoCollection<TEntity> mongoCollection, byte attempt = 1)
        {
            var items = batch?.ToArray();
            try
            {
                await mongoCollection.InsertManyAsync(items,
                    new InsertManyOptions { IsOrdered = false, BypassDocumentValidation = true });
                return true;
            }
            catch (MongoCommandException e)
            {
                switch (e.Code)
                {
                    case 16500: //Request Rate too Large
                        // If attempt failed, try split items in half and try again
                        var batchCount = items.Length / 2;

                        // If we are down to less than 10 items something must be wrong
                        if (batchCount > 5)
                        {
                            var addTasks = new List<Task<bool>>
                            {
                                AddBatchAsync(items.Take(batchCount)),
                                AddBatchAsync(items.Skip(batchCount))
                            };

                            // Run all tasks async
                            await Task.WhenAll(addTasks);
                            return addTasks.All(t => t.Result);
                        }

                        break;
                }

                Logger.LogError(e.ToString());

                throw;
            }
            catch (Exception e)
            {
                if (attempt < 3)
                {
                    Logger.LogWarning($"Add batch to mongodb collection ({MongoCollection}). Attempt {attempt} failed. Tries again...");
                    Thread.Sleep(attempt * 1000);
                    attempt++;
                    return await AddBatchAsync(items, mongoCollection, attempt);
                }

                Logger.LogError(e, "Failed to add batch to mongodb collection ({@mongoCollection})", MongoCollection);
                throw;
            }
        }

        /// <summary>
        /// Name of collection
        /// </summary>
        protected virtual string CollectionName => $"{_collectionName}{(Mode == JobRunModes.IncrementalActiveInstance ? "_incrementalActive" : Mode == JobRunModes.IncrementalInactiveInstance ? "_incrementalInactive" : "")}";

        protected readonly IMongoDbClient Client;

        /// <summary>
        ///     Mongo db
        /// </summary>
        protected readonly IMongoDatabase Database;

        /// <summary>
        ///     Logger
        /// </summary>
        protected readonly ILogger Logger;

        public IMongoCollection<TEntity> GetMongoCollection(string collectionName)
        {
            return Database.GetCollection<TEntity>(collectionName)
                .WithWriteConcern(new WriteConcern(1, journal: true));
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        protected RepositoryBase(
            IMongoDbClient client,
            ILogger logger
        ) : this(client, typeof(TEntity).Name, logger)
        {
           
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="collectionName"></param>
        /// <param name="logger"></param>
        protected RepositoryBase(
            IMongoDbClient client,
            string collectionName,
            ILogger logger
        ) 
        {
            BsonNetTopologySuiteSerializers.Register();

            Client = client ?? throw new ArgumentNullException(nameof(client));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Database = client.GetDatabase();

            BatchSizeRead = client.ReadBatchSize;
            BatchSizeWrite = client.WriteBatchSize;

            // Clean name from non alfa numeric chats
            _collectionName = collectionName.UntilNonAlfanumeric();
        } 


        /// <summary>
        ///     Get client
        /// </summary>
        /// <returns></returns>
        protected IMongoCollection<TEntity> MongoCollection => GetMongoCollection(CollectionName);

        /// <inheritdoc />
        public virtual async Task<bool> AddAsync(TEntity item)
        {
            return await AddAsync(item, MongoCollection);
        }

        /// <inheritdoc />
        public virtual async Task<bool> AddAsync(TEntity item, IMongoCollection<TEntity> mongoCollection)
        {
            try
            {
                await mongoCollection.InsertOneAsync(item);

                return true;
            }
            catch (MongoWriteException e)
            {
                Logger.LogError(e, "Failed to add item");
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to add item");
                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task<bool> AddCollectionAsync()
        {
            return await AddCollectionAsync(CollectionName);
        }

        /// <inheritdoc />
        public async Task<bool> AddCollectionAsync(string collectionName)
        {
            try
            {
                // Create the collection
                await Database.CreateCollectionAsync(collectionName, new CreateCollectionOptions { 
                    Collation = new Collation(locale: "sv", strength: CollationStrength.Primary),
                    IndexOptionDefaults = new IndexOptionDefaults()
                });
                Logger.LogInformation($"The following MongoDB collection was created: [{collectionName}]");

                return true;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to add collection");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task AddIndexes(IEnumerable<CreateIndexModel<TEntity>> indexModels)
        {
            await MongoCollection.Indexes.CreateManyAsync(indexModels);
        }

        /// <inheritdoc />
        public virtual async Task<bool> AddManyAsync(IEnumerable<TEntity> items)
        {
            return await AddManyAsync(items, MongoCollection);
        }

        public async Task WaitForDataInsert(long expectedRecordsCount, TimeSpan? timeout = null)
        {
            Logger.LogInformation($"Begin waiting for MongoDB data. Collection={MongoCollection}, ExpectedRecordsCount={expectedRecordsCount}, Timeout={timeout}");
            if (timeout == null) timeout = TimeSpan.FromMinutes(10);
            var sleepTime = TimeSpan.FromSeconds(5);
            int nrIterations = (int)(Math.Ceiling(timeout.Value.TotalSeconds / sleepTime.TotalSeconds));
            long docCount = await CountAllDocumentsAsync(estimateCount: false);
            var iterations = 0;

            // Compare number of documents retrieved with actually db count
            // If docCount is less than process count, indexing is not ready yet
            while (docCount < expectedRecordsCount && iterations < nrIterations)
            {
                iterations++; // Safety to prevent infinite loop.                                
                await Task.Delay(sleepTime);
                docCount = await CountAllDocumentsAsync(estimateCount: false);
            }

            if (iterations == nrIterations)
            {
                Logger.LogError("Failed waiting for index creation due to timeout. Collection={@mongoCollection}. ExpectedRecordsCount={@expectedRecordsCount}, DocCount={@docCount}",
                    MongoCollection, expectedRecordsCount, docCount);
            }
            else
            {
                Logger.LogInformation("Finish waiting for index creation. Collection={@mongoCollection}.", MongoCollection);
            }
        }

        public async Task<bool> UpsertManyAsync(IEnumerable<TEntity> items, string comparisionField = "_id")
        {
            return await UpsertManyAsync(items, MongoCollection, comparisionField);
        }

        public async Task<bool> UpsertManyAsync(IEnumerable<TEntity> items, IMongoCollection<TEntity> mongoCollection, string comparisionField = "_id")
        {
            if (!items?.Any() ?? true)
            {
                return true;
            }

            var success = true;
            var count = 0;
            var entities = items.ToArray();
            var batch = entities.Skip(0).Take(BatchSizeWrite)?.ToArray();

            while (batch?.Any() ?? false)
            {
                success = success && await UpsertBatchAsync(batch, mongoCollection, comparisionField);
                count++;
                batch = entities.Skip(BatchSizeWrite * count).Take(BatchSizeWrite)?.ToArray();
            }

            return success;
        }

        private async Task<bool> UpsertBatchAsync(
            IEnumerable<TEntity> batch,
            IMongoCollection<TEntity> mongoCollection,
            string comparisonField = "_id",
            byte attempt = 1)
        {
            var items = batch?.ToArray();
            try
            {
                var bulkOps = new List<WriteModel<TEntity>>();
                if (comparisonField != "_id")
                {                    
                    // Fetch all existing documents from MongoDB based on comparisonField
                    var comparisonValues = items.Select(i => typeof(TEntity).GetProperty(comparisonField)?.GetValue(i)).Where(v => v != null).ToList();
                    var existingDocs = await mongoCollection.Find(Builders<TEntity>.Filter.In(comparisonField, comparisonValues)).ToListAsync();                    
                    var existingDocsDict = existingDocs.ToDictionary(doc => typeof(TEntity).GetProperty(comparisonField)?.GetValue(doc));

                    // Update the batch objects with correct _id if they already exist in the database
                    var compProperty = typeof(TEntity).GetProperty(comparisonField);
                    foreach (var item in items)
                    {
                        var comparisonValue = compProperty?.GetValue(item);
                        if (comparisonValue != null && existingDocsDict.TryGetValue(comparisonValue, out var existingDoc))
                        {
                            item.Id = existingDoc.Id;
                            //var idValue = typeof(TEntity).GetProperty("_id")?.GetValue(existingDoc);
                            //if (idValue != null)
                            //{
                            //    typeof(TEntity).GetProperty("_id")?.SetValue(item, idValue);
                            //}
                        }
                    }
                }

                foreach (var item in items)
                {                    
                    var comparisonValue = typeof(TEntity).GetProperty(comparisonField)?.GetValue(item);
                    if (comparisonValue == null)
                        throw new ArgumentException($"Field '{comparisonField}' not found or has null value.");
                    var filter = Builders<TEntity>.Filter.Eq(comparisonField, comparisonValue);
                    var upsert = new ReplaceOneModel<TEntity>(filter, item) { IsUpsert = true };
                    bulkOps.Add(upsert);
                }

                await mongoCollection.BulkWriteAsync(bulkOps, new BulkWriteOptions { IsOrdered = false });
                return true;
            }
            catch (MongoCommandException e)
            {
                switch (e.Code)
                {
                    case 16500: // Request Rate too Large
                        var batchCount = items.Length / 2;
                        if (batchCount > 5)
                        {
                            var addTasks = new List<Task<bool>>
                    {
                        UpsertBatchAsync(items.Take(batchCount), mongoCollection, comparisonField),
                        UpsertBatchAsync(items.Skip(batchCount), mongoCollection, comparisonField)
                    };

                            await Task.WhenAll(addTasks);
                            return addTasks.All(t => t.Result);
                        }
                        break;
                }

                Logger.LogError(e.ToString());
                throw;
            }
            catch (Exception e)
            {
                if (attempt < 3)
                {
                    Logger.LogWarning($"Upsert batch to MongoDB collection ({mongoCollection.CollectionNamespace}). Attempt {attempt} failed. Retrying...");
                    Thread.Sleep(attempt * 1000);
                    attempt++;
                    return await UpsertBatchAsync(items, mongoCollection, comparisonField, attempt);
                }

                Logger.LogError(e, "Failed to upsert batch to MongoDB collection ({@mongoCollection})", mongoCollection);
                throw;
            }
        }



        /// <inheritdoc />
        public async Task<bool> AddManyAsync(IEnumerable<TEntity> items, IMongoCollection<TEntity> mongoCollection)
        {
            if (!items?.Any() ?? true)
            {
                return true;
            }

            var success = true;
            var count = 0;
            var entities = items.ToArray();
            var batch = entities.Skip(0).Take(BatchSizeWrite)?.ToArray();

            while (batch?.Any() ?? false)
            {
                success = success && await AddBatchAsync(batch, mongoCollection);
                count++;
                batch = entities.Skip(BatchSizeWrite * count).Take(BatchSizeWrite)?.ToArray();
            }

            return success;
        }

        /// <inheritdoc />
        public virtual async Task<bool> AddOrUpdateAsync(TEntity item)
        {
            return await AddOrUpdateAsync(item, MongoCollection);
        }

        /// <inheritdoc />
        public async Task<bool> AddOrUpdateAsync(TEntity item, IMongoCollection<TEntity> mongoCollection)
        {
            var filter = Builders<TEntity>.Filter.Eq("_id", item.Id);

            var entity = await mongoCollection.Find(filter).FirstOrDefaultAsync();
            if (entity == null)
            {
                return await AddAsync(item);
            }

            return await UpdateAsync(item.Id, item);
        }

        /// <inheritdoc />
        public int BatchSizeRead { get; set; }

        /// <inheritdoc />
        public int BatchSizeWrite { get; set; }

        /// <inheritdoc />
        public virtual async Task<bool> CheckIfCollectionExistsAsync()
        {
            return await CheckIfCollectionExistsAsync(CollectionName);
        }

        /// <inheritdoc />
        public async Task<bool> CheckIfCollectionExistsAsync(string collectionName)
        {
            try
            {
                var options = new ListCollectionNamesOptions
                {
                    Filter = new BsonDocument("name", collectionName)
                };

                using (var cancellationTokenSource = new CancellationTokenSource(_clientTimeout)) // Client-side timeout
                {
                    var exists = await (await Database
                            .ListCollectionNamesAsync(options, cancellationTokenSource.Token))
                        .AnyAsync(cancellationTokenSource.Token);

                    return exists;
                }
            }
            catch (OperationCanceledException)
            {
                Logger.LogError("MongoDB operation was canceled due to client-side timeout.");
                throw;
            }
            catch (MongoExecutionTimeoutException)
            {
                Logger.LogError("MongoDB operation exceeded the server-side timeout.");
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to check if collection exists with timeout.");
                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task<bool> DeleteAsync(TKey id)
        {
            return await DeleteAsync(id, MongoCollection);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(TKey id, IMongoCollection<TEntity> mongoCollection)
        {
            try
            {
                var removeFilter = Builders<TEntity>.Filter.Eq("_id", id);
                var deleteResult = await mongoCollection.DeleteOneAsync(removeFilter);

                return deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to delete item");

                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task<bool> DeleteCollectionAsync()
        {
            return await DeleteCollectionAsync(CollectionName);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteCollectionAsync(string collectionName)
        {
            try
            {
                // Create the collection
                await Database.DropCollectionAsync(collectionName);
                Logger.LogInformation($"The following MongoDB collection was deleted: [{collectionName}]");

                return true;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to delete collection");
                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task<long> CountAllDocumentsAsync(bool estimateCount = true)
        {
            return await CountAllDocumentsAsync(MongoCollection, estimateCount);
        }

        /// <inheritdoc />
        public async Task<long> CountAllDocumentsAsync(IMongoCollection<TEntity> mongoCollection, bool estimateCount = true)
        {            
            try
            {        
                var options = new CountOptions
                {
                    MaxTime = _serverTimeout // Server-side timeout
                };

                using (var cancellationTokenSource = new CancellationTokenSource(_clientTimeout)) // Client-side timeout
                {
                    if (estimateCount)
                    {
                        return await mongoCollection.EstimatedDocumentCountAsync(new EstimatedDocumentCountOptions
                        {
                            MaxTime = _serverTimeout
                        }, cancellationTokenSource.Token);
                    }

                    return await MongoCollection.CountDocumentsAsync(FilterDefinition<TEntity>.Empty, options, cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                Logger.LogError("MongoDB operation was canceled due to client-side timeout.");
                throw;
            }
            catch (MongoExecutionTimeoutException)
            {
                Logger.LogError("MongoDB operation exceeded the server-side timeout.");
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to count documents with timeout.");
                //throw;
                return 0;
            }
        }         

        /// <inheritdoc />
        public virtual async Task<bool> DeleteManyAsync(IEnumerable<TKey> ids)
        {
            return await DeleteManyAsync(ids, MongoCollection);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteManyAsync(IEnumerable<TKey> ids, IMongoCollection<TEntity> mongoCollection)
        {
            try
            {
                var res = await mongoCollection.Find(x => ids.Contains(x.Id)).ToListAsync();
                if (res != null && res.Any())
                {
                    var removeFilter = Builders<TEntity>.Filter.In("_id", res.Select(x => x.Id));
                    var deleteResult = await mongoCollection // todo - is this correct?
                        .DeleteManyAsync(removeFilter);
                    //var deleteResult = await Database.GetCollection<TEntity>(typeof(TEntity).Name)
                    //    .DeleteManyAsync(removeFilter);
                    return deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0;
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to delete batch");

                throw;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> GetAsync(TKey id)
        {
            try
            {
                var searchFilter = Builders<TEntity>.Filter.Eq("_id", id);
                var options = new FindOptions
                {                    
                    MaxTime = _serverTimeout // Server-side timeout
                };

                using (var cancellationTokenSource = new CancellationTokenSource(_clientTimeout)) // Client-side timeout
                {
                    var result = await MongoCollection
                        .Find(searchFilter, options)                        
                        .FirstOrDefaultAsync(cancellationTokenSource.Token);
                    return result;
                }
            }
            catch (OperationCanceledException)
            {
                Logger.LogError("MongoDB operation was canceled due to client-side timeout.");
                throw;
            }
            catch (MongoExecutionTimeoutException)
            {
                Logger.LogError("MongoDB operation exceeded the server-side timeout.");
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to get item by id");
                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> GetAsync(IEnumerable<TKey> ids)
        {
            try
            {
                var options = new FindOptions<TEntity>
                {
                    MaxTime = _serverTimeout
                };

                using (var cancellationTokenSource = new CancellationTokenSource(_clientTimeout))
                {
                    var cursor = await MongoCollection.FindAsync(x => ids.Contains(x.Id), options, cancellationTokenSource.Token);
                    return await cursor.ToListAsync(cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                Logger.LogError("MongoDB operation was canceled due to client-side timeout.");
                throw;
            }
            catch (MongoExecutionTimeoutException)
            {
                Logger.LogError("MongoDB operation exceeded the server-side timeout.");
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to get many by id's with timeout.");
                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> GetBatchAsync(int skip)
        {
            return await GetBatchAsync(skip, MongoCollection);
        }

        public async Task<IEnumerable<TEntity>> GetBatchAsync(int skip, IMongoCollection<TEntity> mongoCollection)
        {
            try
            {
                var options = new FindOptions
                {
                    MaxTime = _serverTimeout // Server-side timeout
                };

                using (var cancellationTokenSource = new CancellationTokenSource(_clientTimeout)) // Client-side timeout
                {
                    var res = await mongoCollection
                        .Find(FilterDefinition<TEntity>.Empty, options)
                        //.Sort(Builders<TEntity>.Sort.Descending("id"))
                        .Skip(skip)
                        .Limit(BatchSizeRead)
                        .ToListAsync(cancellationTokenSource.Token);

                    return res;
                }
            }
            catch (OperationCanceledException)
            {
                Logger.LogError("MongoDB operation was canceled due to client-side timeout.");
                throw;
            }
            catch (MongoExecutionTimeoutException)
            {
                Logger.LogError("MongoDB operation exceeded the server-side timeout.");
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to get batch");
                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task<List<TEntity>> GetAllAsync()
        {
            return await GetAllAsync(MongoCollection);
        }

        /// <inheritdoc />
        public async Task<List<TEntity>> GetAllAsync(IMongoCollection<TEntity> mongoCollection)
        {
            try
            {
                var options = new FindOptions<TEntity>
                {
                    MaxTime = _serverTimeout // Server-side timeout
                };

                using (var cancellationTokenSource = new CancellationTokenSource(_clientTimeout)) // Client-side timeout
                {
                    var cursor = await mongoCollection.FindAsync(FilterDefinition<TEntity>.Empty, options, cancellationTokenSource.Token);
                    return await cursor.ToListAsync(cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                Logger.LogError("MongoDB operation was canceled due to client-side timeout.");
                throw;
            }
            catch (MongoExecutionTimeoutException)
            {
                Logger.LogError("MongoDB operation exceeded the server-side timeout.");
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to retrieve all documents with timeout.");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IAsyncCursor<TEntity>> GetAllByCursorAsync()
        {
            return await GetAllByCursorAsync(MongoCollection);
        }

        /// <inheritdoc />
        public async Task<IAsyncCursor<TEntity>> GetAllByCursorAsync(IMongoCollection<TEntity> mongoCollection,
            bool noCursorTimeout = false)
        {
            return await mongoCollection.FindAsync(FilterDefinition<TEntity>.Empty,
                new FindOptions<TEntity, TEntity> { MaxTime = _serverTimeout, NoCursorTimeout = noCursorTimeout, BatchSize = BatchSizeRead, AllowPartialResults = true, CursorType = CursorType.NonTailable });
        }

        /// <inheritdoc />
        public async Task<List<TProjection>> GetAllAsync<TProjection>(ProjectionDefinition<TEntity, TProjection> projectionDefinition,
            bool noCursorTimeout = false)
        {
            return await GetAllAsync(MongoCollection, projectionDefinition, noCursorTimeout);
        }

        /// <inheritdoc />
        public async Task<List<TProjection>> GetAllAsync<TProjection>(IMongoCollection<TEntity> mongoCollection,
            ProjectionDefinition<TEntity, TProjection> projectionDefinition,
            bool noCursorTimeout = false)
        {
            var items = new List<TProjection>();
            var cursor = await GetAllByCursorAsync(mongoCollection, projectionDefinition, noCursorTimeout);
            while (await cursor.MoveNextAsync())
            {
                items.AddRange(cursor.Current);
            }

            return items;
        }

        /// <inheritdoc />
        public async Task<IAsyncCursor<TProjection>> GetAllByCursorAsync<TProjection>(ProjectionDefinition<TEntity, TProjection> projectionDefinition,
            bool noCursorTimeout = false)
        {
            return await GetAllByCursorAsync(MongoCollection, projectionDefinition, noCursorTimeout);
        }

        /// <inheritdoc />
        public async Task<IAsyncCursor<TProjection>> GetAllByCursorAsync<TProjection>(IMongoCollection<TEntity> mongoCollection,
            ProjectionDefinition<TEntity, TProjection> projectionDefinition,
            bool noCursorTimeout = false)
        {
            return await GetByCursorAsync(mongoCollection,
                FilterDefinition<TEntity>.Empty,
                projectionDefinition,
                noCursorTimeout);
        }

        /// <inheritdoc />
        public async Task<IAsyncCursor<TProjection>> GetByCursorAsync<TProjection>(IMongoCollection<TEntity> mongoCollection,
            FilterDefinition<TEntity> filterDefinition,
            ProjectionDefinition<TEntity, TProjection> projectionDefinition,
            bool noCursorTimeout = false)
        {
            return await mongoCollection.FindAsync(filterDefinition,
                new FindOptions<TEntity, TProjection> { MaxTime = _serverTimeout, NoCursorTimeout = noCursorTimeout, BatchSize = BatchSizeRead, AllowPartialResults = true, CursorType = CursorType.NonTailable, Projection = projectionDefinition });
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> GetBatchAsync(TKey startId, TKey endId)
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

                var options = new FindOptions<TEntity>
                {
                    MaxTime = _serverTimeout
                };

                using (var cancellationTokenSource = new CancellationTokenSource(_clientTimeout))
                {
                    var cursor = await mongoCollection
                        .FindAsync(Builders<TEntity>.Filter.And(filters), options, cancellationTokenSource.Token);
                    return await cursor.ToListAsync(cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                Logger.LogError("MongoDB operation was canceled due to client-side timeout.");
                throw;
            }
            catch (MongoExecutionTimeoutException)
            {
                Logger.LogError("MongoDB operation exceeded the server-side timeout.");
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to get batch with timeout.");
                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task<TKey> GetMaxIdAsync()
        {
            return await GetMaxIdAsync(MongoCollection);
        }

        /// <inheritdoc />
        public async Task<TKey> GetMaxIdAsync(IMongoCollection<TEntity> mongoCollection)
        {
            try
            {
                Logger.LogDebug($"Try to get max id for ({mongoCollection.CollectionNamespace})");

                var options = new FindOptions
                {
                    MaxTime = _serverTimeout // Server-side timeout
                };

                using (var cancellationTokenSource = new CancellationTokenSource(_clientTimeout)) // Client-side timeout
                {
                    var max = await mongoCollection
                        .Find(FilterDefinition<TEntity>.Empty, options)
                        .Project(d => d.Id)
                        .Sort(Builders<TEntity>.Sort.Descending("_id"))
                        .Limit(1)
                        .FirstOrDefaultAsync(cancellationTokenSource.Token);

                    return max;
                }
            }
            catch (OperationCanceledException)
            {
                Logger.LogError("MongoDB operation was canceled due to client-side timeout.");
                throw;
            }
            catch (MongoExecutionTimeoutException)
            {
                Logger.LogError("MongoDB operation exceeded the server-side timeout.");
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to get max id");
                throw;
            }
        }

        public async Task<(int Id, T AggregationValue)> GetMaxValueWithIdAsync<T>(
            IMongoCollection<TEntity> mongoCollection,
            string fieldName) where T : IConvertible
        {
            try
            {
                Logger.LogDebug($"Trying to get max value for field '{fieldName}' in ({mongoCollection.CollectionNamespace})");

                var pipeline = new[]
                {
                    new BsonDocument("$sort", new BsonDocument
                    {
                        { fieldName, -1 },
                        { "_id", -1 }
                    }),
                    new BsonDocument("$limit", 1),
                    new BsonDocument("$project", new BsonDocument
                    {
                        { "_id", 1 },
                        { fieldName, 1 }
                    })
                };

                using (var cancellationTokenSource = new CancellationTokenSource(_clientTimeout)) // Apply timeout
                {
                    var result = await mongoCollection.Aggregate<BsonDocument>(pipeline, cancellationToken: cancellationTokenSource.Token).FirstOrDefaultAsync();

                    if (result != null && result.Contains("_id") && result.Contains(fieldName))
                    {
                        int id = result["_id"].AsInt32;
                        T fieldValue = ConvertToType<T>(result[fieldName]);
                        return (id, fieldValue);
                    }

                    return default;
                }
            }
            catch (OperationCanceledException)
            {
                Logger.LogError("MongoDB operation was canceled due to client-side timeout.");
                throw;
            }
            catch (MongoExecutionTimeoutException)
            {
                Logger.LogError("MongoDB operation exceeded the server-side timeout.");
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to get max value for field '{fieldName}'");
                throw;
            }
        }

        private T ConvertToType<T>(BsonValue bsonValue)
        {
            if (bsonValue.IsBsonNull)
                return default;

            return (T)Convert.ChangeType(bsonValue.ToString(), typeof(T)); // Konvertera till T
        }

        public async Task<TKey> GetMaxIdAsync(string collectionName)
        {
            var collection = GetMongoCollection(collectionName);
            return await GetMaxIdAsync(collection);
        }


        public async Task<TEntity> GetDocumentWithMaxIdAsync(IMongoCollection<TEntity> mongoCollection)
        {
            try
            {
                var options = new FindOptions
                {
                    MaxTime = _serverTimeout // Server-side timeout
                };

                using (var cancellationTokenSource = new CancellationTokenSource(_clientTimeout)) // Client-side timeout
                {
                    var document = await mongoCollection
                        .Find(FilterDefinition<TEntity>.Empty, options)
                        .Sort(Builders<TEntity>.Sort.Descending("_id"))
                        .Limit(1)
                        .FirstOrDefaultAsync(cancellationTokenSource.Token);

                    if (document == null)
                    {
                        Logger.LogWarning($"No document found in collection {mongoCollection}.");
                    }

                    return document;
                }
            }
            catch (OperationCanceledException)
            {
                Logger.LogError("MongoDB operation was canceled due to client-side timeout.");
                throw;
            }
            catch (MongoExecutionTimeoutException)
            {
                Logger.LogError("MongoDB operation exceeded the server-side timeout.");
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to fetch document with max id from {mongoCollection}");
                throw;
            }
        }

        /// <inheritdoc />
        public JobRunModes Mode { get; set; }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> QueryAsync(FilterDefinition<TEntity> filter)
        {
            try
            {
                var options = new FindOptions
                {
                    MaxTime = _serverTimeout // Server-side timeout
                };

                using (var cancellationTokenSource = new CancellationTokenSource(_clientTimeout)) // Client-side timeout
                {
                    return await MongoCollection
                        .Find(filter, options)
                        .ToListAsync(cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                Logger.LogError("MongoDB operation was canceled due to client-side timeout.");
                throw;
            }
            catch (MongoExecutionTimeoutException)
            {
                Logger.LogError("MongoDB operation exceeded the server-side timeout.");
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task<bool> UpdateAsync(TKey id, TEntity entity)
        {
            return await UpdateAsync(id, entity, MongoCollection);
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(TKey id, TEntity entity, IMongoCollection<TEntity> mongoCollection)
        {
            try
            {
                var updateResult = await mongoCollection.ReplaceOneAsync(
                    x => x.Id.Equals(id),
                    entity,
                    new ReplaceOptions { IsUpsert = true });
                return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to update entity");
                throw;
            }
        }

        public async Task<bool> AddCollectionAsync(IMongoCollection<TEntity> mongoCollection)
        {
            return await AddCollectionAsync(mongoCollection.CollectionNamespace.CollectionName);
        }

        public async Task<bool> DeleteCollectionAsync(IMongoCollection<TEntity> mongoCollection)
        {
            return await DeleteCollectionAsync(mongoCollection.CollectionNamespace.CollectionName);
        }
    }
}