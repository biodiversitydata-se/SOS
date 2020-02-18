using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Search.Service.Database.Interfaces;
using SOS.Search.Service.Repositories.Interfaces;

namespace SOS.Search.Service.Repositories
{
    /// <summary>
    /// Field mappings repository.
    /// </summary>
    public class ProcessedFieldMappingRepository : ProcessBaseRepository<FieldMapping, FieldMappingFieldId>, IProcessedFieldMappingRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public ProcessedFieldMappingRepository(
            IProcessClient client,
            ILogger<ProcessBaseRepository<FieldMapping, FieldMappingFieldId>> logger) : base(client, true, logger)
        {
            
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
