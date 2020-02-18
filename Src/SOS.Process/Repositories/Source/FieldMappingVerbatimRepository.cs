using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization;
using  SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Shared;
using SOS.Process.Database.Interfaces;

namespace SOS.Process.Repositories.Source
{
    public class FieldMappingVerbatimRepository : VerbatimBaseRepository<FieldMapping, int>, Interfaces.IFieldMappingVerbatimRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public FieldMappingVerbatimRepository(IVerbatimClient client,
            ILogger<FieldMappingVerbatimRepository> logger) : base(client, logger)
        {
           
        }

        public async Task<IEnumerable<FieldMapping>> GetFieldMappingsAsync()
        {
            var skip = 0;
            var fieldMappingsBatch = await GetBatchBySkipAsync(skip);
            var fieldMappings = new List<FieldMapping>();

            while (fieldMappingsBatch?.Any() ?? false)
            {
                fieldMappings.AddRange(fieldMappingsBatch);
                skip += fieldMappingsBatch.Count();
                fieldMappingsBatch = await GetBatchBySkipAsync(skip);
            }

            return fieldMappings;
        }
    }
}