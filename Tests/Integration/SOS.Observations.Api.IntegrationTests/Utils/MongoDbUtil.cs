using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Repositories.Verbatim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Observations.Api.IntegrationTests.Utils
{
    public class MongoDbUtil
    {
        private const int BatchSizeWrite = 10000;
        private IProcessClient fromClient;
        private IProcessClient toClient;

        public MongoDbUtil(IProcessClient fromClient, IProcessClient toClient)
        {
            this.fromClient = fromClient;
            this.toClient = toClient;
        }

        public async Task CopyCollectionAsync<TEntity>(string fromCollectionName, string toCollectionName)
        {
            var fromDb = fromClient.GetDatabase();
            var toDb = toClient.GetDatabase();

            // Check if toCollectionName exists.
            if (await CheckIfCollectionExistsAsync(toDb, toCollectionName)) throw new Exception($"{toCollectionName} already exists.");

            List<TEntity> records = await GetAllRecordsAsync<TEntity>(fromDb, fromCollectionName);
            await AddCollectionAsync(toDb, toCollectionName);
            var toCollection = GetMongoCollection<TEntity>(toDb, toCollectionName);
            foreach (var record in records)
            {
                await AddAsync(record, toCollection);
            }

            // For some reason this way did not work in prod
            //await AddManyAsync<TEntity>(records, toDb, toCollectionName);
        }

        private IMongoCollection<TEntity> GetMongoCollection<TEntity>(IMongoDatabase db, string collectionName)
        {
            return db.GetCollection<TEntity>(collectionName)
                .WithWriteConcern(new WriteConcern(1, journal: true));            
        }

        private async Task<List<TEntity>> GetAllRecordsAsync<TEntity>(IMongoCollection<TEntity> mongoCollection)
        {
            var res = await mongoCollection.AsQueryable().ToListAsync();
            return res;
        }

        private async Task<List<TEntity>> GetAllRecordsAsync<TEntity>(IMongoDatabase db, string collectionName)
        {
            var mongoCollection = GetMongoCollection<TEntity>(db, collectionName);
            var res = await mongoCollection.AsQueryable().ToListAsync();
            return res;
        }


        private async Task<bool> CheckIfCollectionExistsAsync(IMongoDatabase db, string collectionName)
        {
            //filter by collection name
            var exists = await (await db
                    .ListCollectionNamesAsync(new ListCollectionNamesOptions
                    {
                        Filter = new BsonDocument("name", collectionName)
                    }))
                .AnyAsync();

            return exists;
        }

        private async Task AddCollectionAsync(IMongoDatabase db, string collectionName)
        {
            // Create the collection
            await db.CreateCollectionAsync(collectionName);
        }

        public async Task<bool> AddAsync<TEntity>(TEntity item, IMongoCollection<TEntity> mongoCollection)
        {
            try
            {
                await mongoCollection.InsertOneAsync(item);

                return true;
            }
            catch (MongoWriteException e1)
            {
                // Item allready exists
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private async Task<bool> AddManyAsync<TEntity>(IEnumerable<TEntity> items, IMongoDatabase db, string collectionName)
        {
            var mongoCollection = GetMongoCollection<TEntity>(db, collectionName);
            return await AddManyAsync(items, mongoCollection);
        }

        private async Task<bool> AddManyAsync<TEntity>(IEnumerable<TEntity> items, IMongoCollection<TEntity> mongoCollection)
        {
            var entities = items?.ToArray();
            if (!entities?.Any() ?? true)
            {
                return false;
            }

            var success = true;
            var count = 0;
            var batch = entities.Skip(0).Take(BatchSizeWrite)?.ToArray();

            while (batch?.Any() ?? false)
            {
                success = success && await AddBatchAsync(batch, mongoCollection);
                count++;
                batch = entities.Skip(BatchSizeWrite * count).Take(BatchSizeWrite)?.ToArray();
            }

            return success;
        }

        private async Task<bool> AddBatchAsync<TEntity>(IEnumerable<TEntity> batch, IMongoCollection<TEntity> mongoCollection)
        {
            var items = batch?.ToArray();
            try
            {
                await mongoCollection.InsertManyAsync(batch,
                    new InsertManyOptions { IsOrdered = false, BypassDocumentValidation = true });
                return true;
            }
            catch (MongoCommandException e)
            {
                switch (e.Code)
                {
                    case 16500: //Request Rate too Large
                        // If attempt failed, try split items in half and try again
                        var batchCount = batch.Count() / 2;

                        // If we are down to less than 10 items something must be wrong
                        if (batchCount > 5)
                        {
                            var addTasks = new List<Task<bool>>
                            {
                                AddBatchAsync(mongoCollection, batch.Take(batchCount)),
                                AddBatchAsync(mongoCollection, batch.Skip(batchCount))
                            };

                            // Run all tasks async
                            await Task.WhenAll(addTasks);
                            return addTasks.All(t => t.Result);
                        }

                        break;
                }

                throw;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private async Task<bool> AddBatchAsync<TEntity>(IMongoCollection<TEntity> mongoCollection, IEnumerable<TEntity> batch)
        {
            return await AddBatchAsync(batch, mongoCollection);
        }
    }
}