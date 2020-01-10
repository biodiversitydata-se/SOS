﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Export.MongoDb.Interfaces;
using SOS.Export.Repositories.Interfaces;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.DarwinCore;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Verbatim.SpeciesPortal;

namespace SOS.Export.Repositories
{
    /// <summary>
    /// Species data service
    /// </summary>
    public class ProcessedDarwinCoreRepository : BaseRepository<DarwinCore<DynamicProperties>, ObjectId>, IProcessedDarwinCoreRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="exportClient"></param>
        /// <param name="logger"></param>
        public ProcessedDarwinCoreRepository(
            IExportClient exportClient,
            ILogger<ProcessedDarwinCoreRepository> logger) : base(exportClient, true, logger)
        {
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DarwinCore<DynamicProperties>>> GetChunkAsync(AdvancedFilter filter, int skip, int take)
        {
            var res = await MongoCollection
                .Find(filter.ToFilterDefinition())
                .Skip(skip)
                .Limit(take)
                .ToListAsync();

            return res;
        }

        public async Task<IEnumerable<DarwinCoreProject>> GetProjectParameters(
            AdvancedFilter filter, 
            int skip,
            int take)
        {
            List<IEnumerable<DarwinCoreProject>> res = await MongoCollection
                .Find(filter.ToProjectParameteFilterDefinition())
                .Skip(skip)
                .Limit(take)
                .Project(x => x.DynamicProperties.Projects)
                .ToListAsync();

            var projectParameters = res.SelectMany(pp => pp);
            return projectParameters;
        }
    }
}
