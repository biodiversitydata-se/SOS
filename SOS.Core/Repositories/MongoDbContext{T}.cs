using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;

namespace SOS.Core.Repositories
{
    public class MongoDbContext<T>
    {
        private readonly MongoClient _mongodbClient;
        private readonly IMongoDatabase _mongodb;

        public MongoDbContext(string connStr, string databaseName, string collectionName)
        {
            _mongodbClient = new MongoClient(connStr);
            _mongodb = _mongodbClient.GetDatabase(databaseName);
            MongoDbCollection = _mongodb.GetCollection<T>(collectionName);
        }

        public IMongoCollection<T> MongoDbCollection { get; }

        public MongoClient MongodbClient => _mongodbClient;

        public IMongoDatabase Mongodb => _mongodb;
    }
}
