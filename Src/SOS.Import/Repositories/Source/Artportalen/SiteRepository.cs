using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using SOS.Import.Entities.Artportalen;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Extensions;

namespace SOS.Import.Repositories.Source.Artportalen
{
    /// <summary>
	///     Site repository
	/// </summary>
	public class SiteRepository : BaseRepository<SiteRepository>, ISiteRepository
    {
        private string GetSiteQuery(string join) =>
            $@"
                SELECT 
	                s.Id,
	                ISNULL(s.PresentationName, s.Name) AS Name,
	                s.XCoord,
	                s.YCoord,
                    s.Accuracy,
				    s.ExternalId,
				    s.ParentId AS ParentSiteId,
				    s.PresentationNameParishRegion
                FROM 
	                Site s 
                    { join }";


        /// <summary>
        /// Get sites by id's, Up to 3 attempts will be made if call fails
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="attempt"></param>
        /// <param name="live"></param>
        /// <returns></returns>
        private async Task<IEnumerable<SiteEntity>> GetByIdsAsync(IEnumerable<int> ids, bool live, int attempt)
        {
            try
            {
                return await QueryAsync<SiteEntity>(GetSiteQuery(
                        $"INNER JOIN @tvp t ON s.Id = t.Id"),
                    new { tvp = ids.ToDataTable().AsTableValuedParameter("dbo.IdValueTable") }, live);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting sites by id");

                if (attempt < 2)
                {
                    return await GetByIdsAsync(ids, live, ++attempt);
                }
                
                return null;
            }
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="artportalenDataService"></param>
        /// <param name="logger"></param>
        public SiteRepository(IArtportalenDataService artportalenDataService, ILogger<SiteRepository> logger) : base(
            artportalenDataService, logger)
        {
        }

        /// <inheritdoc />
        public async Task<IDictionary<int, ICollection<AreaEntityBase>>> GetSitesAreas(IEnumerable<int> siteIds)
        {
            if (!siteIds?.Any() ?? true)
            {
                return null;
            }

            try
            {
                const string query = @"
                SELECT 
                    sa.SiteId,
                    a.AreaDatasetId,
		            a.FeatureId,
                    a.Name
	            FROM 
		            SiteAreas sa
		            INNER JOIN Area a ON sa.AreasId = a.Id 
                    INNER JOIN @sid s ON sa.SiteId = s.Id
                WHERE
                    a.AreaDatasetId IN (1, 16, 18, 19, 21)";

                var siteAreaEntities = (await QueryAsync<SiteAreaEntity>(query,
                    new { sid = siteIds.ToDataTable().AsTableValuedParameter("dbo.IdValueTable") }))?.ToArray();

                var siteAreas = new Dictionary<int, ICollection<AreaEntityBase>>();
                if (siteAreaEntities?.Any() ?? false)
                {
                    foreach (var siteAreaEntity in siteAreaEntities)
                    {
                        if (!siteAreas.TryGetValue(siteAreaEntity.SiteId, out var areas))
                        {
                            areas = new List<AreaEntityBase>();
                            siteAreas.Add(siteAreaEntity.SiteId, areas);
                        }
                        areas.Add(siteAreaEntity);
                    }
                }
                return siteAreas;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting sites areas");
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SiteEntity>> GetByIdsAsync(IEnumerable<int> ids, bool live = false)
		{
            if (!ids?.Any() ?? true)
            {
                return null;
            }

            return await GetByIdsAsync(ids, live, 0);
        }

        /// <inheritdoc />
        public async Task<IDictionary<int, string>> GetSitesGeometry(IEnumerable<int> siteIds)
        {
            if (!siteIds?.Any() ?? true)
            {
                return null;
            }

            try
            {
                const string query = @"
                SELECT
			        sg.SiteId,
                    MAX(sg.Geometry.STAsText()) AS GeometryWKT -- Ugly workaround to only get one geometry/site. Bad data, duplicates exists
		        FROM 
		            SiteGeometry sg 
                    INNER JOIN @sid s ON sg.SiteId = s.Id
				GROUP BY 
					sg.SiteId";

                var sitesGeometry = (await QueryAsync<(int SiteId, string GeometryWKT)>(query,
                    new { sid = siteIds.ToDataTable().AsTableValuedParameter("dbo.IdValueTable") }))?.ToArray();

                return sitesGeometry?.ToDictionary(sg => sg.SiteId, sg => sg.GeometryWKT);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting sites geometry");
                return null;
            }
        }
    }
}