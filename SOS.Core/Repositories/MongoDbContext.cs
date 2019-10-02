using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;

namespace SOS.Core.Repositories
{
    public class MongoDbContext
    {
        private readonly MongoClient _mongodbClient;
        private readonly IMongoDatabase _mongodb;
        private readonly string _collectionName;

        public MongoDbContext(string connStr, string databaseName, string collectionName)
        {
            _mongodbClient = new MongoClient(connStr);
            _mongodb = _mongodbClient.GetDatabase(databaseName);
            _collectionName = collectionName;
        }

        public string CollectionName => _collectionName;

        public IMongoCollection<T> MongoDbCollection<T>()
        {
            return Mongodb.GetCollection<T>(_collectionName);
        }

        public IMongoCollection<T> MongoDbCollection<T>(string collectionName)
        {
            return Mongodb.GetCollection<T>(collectionName);
        }

        public MongoClient MongodbClient => _mongodbClient;

        public IMongoDatabase Mongodb => _mongodb;
    }
}
