﻿using Dapper;
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
                    ISNULL(ps.PresentationName, ps.Name) AS ParentSiteName,
                    d.Factor AS DiffusionFactor
                FROM 
	                Site s 
                    LEFT JOIN Site ps ON s.ParentId = ps.Id
                    LEFT JOIN Diffusion d ON s.DiffusionId = d.Id
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
                    new { tvp = ids.ToSqlRecords().AsTableValuedParameter("dbo.IdValueTable") }, live);
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
        public async Task<IDictionary<int, ICollection<AreaEntityBase>>?> GetSitesAreas(IEnumerable<int> siteIds, bool live = false)
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
                    a.AreaDatasetId IN (1, 16, 18, 19, 21)";

                var siteAreaEntities = (await QueryAsync<SiteAreaEntity>(query,
                    new { sid = siteIds.ToSqlRecords().AsTableValuedParameter("dbo.IdValueTable") }, live))?.ToArray();

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
        public async Task<IEnumerable<SiteEntity>?> GetByIdsAsync(IEnumerable<int> ids, bool live = false)
        {
            if (!ids?.Any() ?? true)
            {
                return null;
            }

            return await GetByIdsAsync(ids, live, 0);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<int>> GetFreqventlyUsedIdsAsync(bool live)
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
                        HAVING COUNT (s.SiteId) > 1", live);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting frequently used sites");
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<IDictionary<int, string>?> GetSitesGeometry(IEnumerable<int> siteIds, bool live = false)
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
                    new { sid = siteIds.ToSqlRecords().AsTableValuedParameter("dbo.IdValueTable") }, live))?.ToArray();

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