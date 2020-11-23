using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using SOS.Import.Entities.Artportalen;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;

namespace SOS.Import.Repositories.Source.Artportalen
{
    /// <summary>
    ///     Area repository
    /// </summary>
    public class AreaRepository : BaseRepository<AreaRepository>, IAreaRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="artportalenDataService"></param>
        /// <param name="logger"></param>
        public AreaRepository(IArtportalenDataService artportalenDataService, ILogger<AreaRepository> logger) : base(
            artportalenDataService, logger)
        {
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AreaEntity>> GetAsync()
        {
            try
            {
                var areaTypes = ((int[]) Enum.GetValues(typeof(AreaType))).Where(at => at != 25);

                var query = @"
                SELECT -- Break out feature 100 (Sweden) from AreaDataSetId = 18
	                CASE WHEN a.AreaDatasetId = 18 AND a.FeatureId = 100 THEN 25 ELSE a.AreaDatasetId END AS AreaDatasetId,
                    a.Id,
                    a.FeatureId,
                    a.Polygon.STAsText() AS PolygonWKT,
	                a.Name,
                    a.ParentId
                FROM 
	                Area a
                    INNER JOIN @tvp t ON a.AreaDatasetId = t.Id";

                return await QueryAsync<AreaEntity>(query,
                    new { tvp = areaTypes.ToDataTable().AsTableValuedParameter("dbo.IdValueTable") });
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting areas");
                return null;
            }
        }
    }
}