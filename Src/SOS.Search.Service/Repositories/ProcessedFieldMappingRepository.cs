using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.Shared;
using SOS.Search.Service.Repositories.Interfaces;

namespace SOS.Search.Service.Repositories
{
    /// <summary>
    /// Field mappings repository.
    /// </summary>
    public class ProcessedFieldMappingRepository : BaseRepository<FieldMapping, int>, IProcessedFieldMappingRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mongoClient"></param>
        /// <param name="mongoDbConfiguration"></param>
        /// <param name="logger"></param>
        public ProcessedFieldMappingRepository(
            IMongoClient mongoClient, 
            IOptions<MongoDbConfiguration> mongoDbConfiguration,
            ILogger<BaseRepository<FieldMapping, int>> logger) : base(mongoClient, mongoDbConfiguration, true, logger)
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(FieldMappingValue)))
            {
                BsonClassMap.RegisterClassMap<FieldMappingValue>();
                BsonClassMap.RegisterClassMap<FieldMappingWithCategoryValue>();
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<FieldMapping>> GetFieldMappingsAsync()
        {
            try
            {
                const int batchSize = 200000;
                var skip = 0;
                var fieldMappingsChunk = await GetChunkAsync(skip, batchSize);
                var fieldMappings = new List<FieldMapping>();

                while (fieldMappingsChunk?.Any() ?? false)
                {
                    fieldMappings.AddRange(fieldMappingsChunk);
                    skip += fieldMappingsChunk.Count();
                    fieldMappingsChunk = await GetChunkAsync(skip, batchSize);
                }

                return fieldMappings;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to get chunk of field mappings");
                return null;
            }
        }

        /// <summary>
        /// Get chunk of taxa
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        private async Task<IEnumerable<FieldMapping>> GetChunkAsync(int skip, int take)
        {
            var res = await MongoCollection
                .Find(x => true)
                .Skip(skip)
                .Limit(take)
                .ToListAsync();

            return res;
        }
    }
}
