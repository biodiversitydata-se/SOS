using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Import.Entities;
using SOS.Import.Extensions;
using SOS.Import.Models;
using SOS.Import.Models.Aggregates;
using SOS.Import.Repositories.Source.SpeciesPortal.Enums;
using SOS.Import.Services.Interfaces;

namespace SOS.Import.Repositories.Source.SpeciesPortal
{
    public class SightingRelationRepository : BaseRepository<SightingRelationRepository>, Interfaces.ISightingRelationRepository
    {
        public SightingRelationRepository(
            ISpeciesPortalDataService speciesPortalDataService, 
            ILogger<SightingRelationRepository> logger) : base(speciesPortalDataService, logger)
        {
        }

        public async Task<IEnumerable<SightingRelationEntity>> GetAsync(IEnumerable<int> sightingIds)
        {
            try
            {
                var tvpTable = new DataTable();
                tvpTable.Columns.Add(new DataColumn("Id", typeof(int)));
                foreach (var id in sightingIds)
                {
                    tvpTable.Rows.Add(id);
                }

                const string query = @"
                SELECT	                
	                sr.SightingId,
	                sr.UserId,
	                sr.SightingRelationTypeId,
	                sr.Sort,
	                sr.IsPublic,	                
	                sr.DeterminationYear
                FROM
	                [SightingRelation] sr
                INNER JOIN @tvp t ON sr.SightingId = t.Id 
                    AND sr.IsPublic = 1";

                return await QueryAsync<SightingRelationEntity>(query, new {tvp = tvpTable.AsTableValuedParameter("dbo.IdValueTable")});
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting sighting relations");
                return null;
            }
        }
    }
}