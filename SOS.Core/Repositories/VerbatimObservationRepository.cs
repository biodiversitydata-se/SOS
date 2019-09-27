using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using SOS.Core.Models.Versioning;

namespace SOS.Core.Repositories
{
    public class VerbatimObservationRepository<T>
    {
        private readonly MongoDbContext _dbContext;
        public IMongoCollection<T> Collection { get; private set; }

        public VerbatimObservationRepository(MongoDbContext dbContext)
        {
            _dbContext = dbContext;
            Collection = _dbContext.MongoDbCollection<T>();
        }

        public async Task InsertObservationAsync(T doc)
        {
            await Collection.InsertOneAsync(doc);
        }


        public async Task InsertObservationsAsync(IEnumerable<T> observations)
        {
            await Collection.InsertManyAsync(observations);
        }

        public IEnumerable<T> GetAllObservations()
        {
            return Collection.Find(x => true).ToEnumerable();
        }

        public async Task DropVerbatimObservationCollectionAsync()
        {
            await _dbContext.Mongodb.DropCollectionAsync(_dbContext.CollectionName);
        }
    }
}
