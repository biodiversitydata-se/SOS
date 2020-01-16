using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Export.MongoDb.Interfaces;
using SOS.Export.Repositories.Interfaces;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.Search;

namespace SOS.Export.Repositories
{
    /// <summary>
    /// Species data service
    /// </summary>
    public class ProcessedSightingRepository : BaseRepository<ProcessedSighting, ObjectId>, IProcessedSightingRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="exportClient"></param>
        /// <param name="logger"></param>
        public ProcessedSightingRepository(
            IExportClient exportClient,
            ILogger<ProcessedSightingRepository> logger) : base(exportClient, true, logger)
        {
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ProcessedSighting>> GetChunkAsync(AdvancedFilter filter, int skip, int take)
        {
            var res = await MongoCollection
                .Find(filter.ToFilterDefinition())
                .Skip(skip)
                .Limit(take)
                .ToListAsync();

            return res;
        }

        public async Task<IEnumerable<ProcessedProject>> GetProjectParameters(
            AdvancedFilter filter, 
            int skip,
            int take)
        {
            List<IEnumerable<ProcessedProject>> res = await MongoCollection
                .Find(filter.ToProjectParameteFilterDefinition())
                .Skip(skip)
                .Limit(take)
                .Project(x => x.Projects)
                .ToListAsync();

            var projectParameters = res.SelectMany(pp => pp);
            return projectParameters;
        }
    }
}
