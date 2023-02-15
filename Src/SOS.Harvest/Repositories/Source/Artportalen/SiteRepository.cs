using Dapper;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Extensions;

namespace SOS.Harvest.Repositories.Source.Artportalen
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
                    s.TrueXCoord,
                    s.TrueYCoord,
                    s.Accuracy,
				    s.ExternalId,
				    s.ParentId AS ParentSiteId,
				    s.PresentationNameParishRegion,
                    s.ProjectId,
                    s.IsPrivate,
                    s.IncludedBySiteId,
                    ISNULL(ps.PresentationName, ps.Name) AS ParentSiteName,
                    s.DiffusionId
                FROM 
	                Site s 
                    {join}
                    LEFT JOIN Site ps ON s.ParentId = ps.Id";


        /// <summary>
        /// Get sites by id's, Up to 3 attempts will be made if call fails
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="attempt"></param>
        /// <returns></returns>
        private async Task<IEnumerable<SiteEntity>> GetByIdsAsync(IEnumerable<int> ids, int attempt)
        {
            try
            {
                return await QueryAsync<SiteEntity>(GetSiteQuery(
                        $"INNER JOIN @tvp t ON s.Id = t.Id"),
                    new { tvp = ids.ToSqlRecords().AsTableValuedParameter("dbo.IdValueTable") });
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting sites by id");

                if (attempt < 2)
                {
                    return await GetByIdsAsync(ids, ++attempt);
                }

                throw;
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
        public async Task<IDictionary<int, ICollection<AreaEntityBase>>?> GetSitesAreas(IEnumerable<int> siteIds)
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
                    INNER JOIN @sid s ON sa.SiteId = s.Id
		            INNER JOIN Area a ON sa.AreasId = a.Id 
                WHERE
                    a.AreaDatasetId IN (1, 13, 16, 18, 19, 21)";

                var siteAreaEntities = (await QueryAsync<SiteAreaEntity>(query,
                    new { sid = siteIds.ToSqlRecords().AsTableValuedParameter("dbo.IdValueTable") }))?.ToArray();

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
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SiteEntity>?> GetByIdsAsync(IEnumerable<int> ids)
        {
            if (!ids?.Any() ?? true)
            {
                return null;
            }

            return await GetByIdsAsync(ids, 0);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<int>> GetFreqventlyUsedIdsAsync()
        {
            try
            {
                return await QueryAsync<int>(
                        @$"
                        SELECT
                            s.SiteId
                        FROM
                            {SightingsFromBasics}
                        WHERE
                            {SightingWhereBasics}
                        GROUP BY
	                        s.SiteId
                        HAVING COUNT (s.SiteId) > 1");
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting frequently used sites");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IDictionary<int, string>?> GetSitesGeometry(IEnumerable<int> siteIds)
        {
            if (!siteIds?.Any() ?? true)
            {
                return null;
            }

            try
            {
                const string query = @"
                SELECT
	                lg.SiteId,
	                lg.Geometry.STAsText() AS GeometryWKT
                FROM (
	                    SELECT
		                    sg.SiteId,
		                    sg.Geometry,
		                    ROW_NUMBER() OVER (PARTITION BY SiteId ORDER BY EditDate DESC) rn -- Workaround to only get one geometry/site. Bad data, duplicates exists
	                    FROM 
		                    SiteGeometry sg 
	                        INNER JOIN @sid s ON sg.SiteId = s.Id
                    ) AS lg
                WHERE 
	                lg.rn = 1";

                var sitesGeometry = (await QueryAsync<(int SiteId, string GeometryWKT)>(query,
                    new { sid = siteIds.ToSqlRecords().AsTableValuedParameter("dbo.IdValueTable") }))?.ToArray();

                return sitesGeometry?.ToDictionary(sg => sg.SiteId, sg => sg.GeometryWKT);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting sites geometry");
                throw;
            }
        }
    }
}