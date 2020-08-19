using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Entities.Artportalen;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Import.Services.Interfaces;

namespace SOS.Import.Repositories.Source.Artportalen
{
    /// <summary>
	///     Site repository
	/// </summary>
	public class SiteRepository : BaseRepository<SiteRepository>, ISiteRepository
    {
        private string GetSiteQuery(string where = "") =>
            $@"
                SELECT 
	                s.Id,
	                ISNULL(s.PresentationName, s.Name) AS Name,
	                s.XCoord,
	                s.YCoord,
                    s.Accuracy,
					s.ExternalId,
	                am.Id AS MunicipalityId,
	                am.Name AS MunicipalityName,
	                am.ParentId AS CountyId,
	                am.ParentAreaName AS CountyName,
	                ap.Id AS ProvinceId,
	                ap.Name AS ProvinceName,
	                acp.Id AS CountryPartId,
	                acp.Name AS CountryPartName,
	                apa.Id AS ParishId,
	                apa.Name AS ParishName,
					s.ParentId AS ParentSiteId,
					s.PresentationNameParishRegion
                FROM 
	                Site s 
	                LEFT JOIN -- Bad data exists, some sites are connected to more than one Municipality
	                (
		                SELECT
			                sa.SiteId,
			                MAX(sa.AreasId) AS AreasId
		                FROM
			                SiteAreas sa
			                INNER JOIN Area a ON sa.AreasId = a.Id AND a.AreaDatasetId = 1 
		                GROUP BY 
			                sa.SiteId
	                ) AS sam ON s.Id = sam.SiteId  
	                LEFT JOIN Area am ON sam.AreasId = am.Id
	                LEFT JOIN -- Bad data exists, some sites are connected to more than one province
	                (
		                SELECT
			                sa.SiteId,
			                MAX(sa.AreasId) AS AreasId
		                FROM
			                SiteAreas sa
			                INNER JOIN Area a ON sa.AreasId = a.Id AND a.AreaDatasetId = 16 
		                GROUP BY 
			                sa.SiteId
	                ) AS sap ON s.Id = sap.SiteId  
	                LEFT JOIN Area ap ON sap.AreasId = ap.Id
	                LEFT JOIN -- Bad data exists, some sites are connected to more than one country part
	                (
		                SELECT
			                sa.SiteId,
			                MAX(sa.AreasId) AS AreasId
		                FROM
			                SiteAreas sa
			                INNER JOIN Area a ON sa.AreasId = a.Id AND a.AreaDatasetId = 13 
		                GROUP BY 
			                sa.SiteId
	                ) AS sacp ON s.Id = sacp.SiteId  
	                LEFT JOIN Area acp ON sacp.AreasId = acp.Id
	                LEFT JOIN -- Bad data exists, some sites are connected to more than one parish
	                (
		                SELECT
			                sa.SiteId,
			                MAX(sa.AreasId) AS AreasId
		                FROM
			                SiteAreas sa
			                INNER JOIN Area a ON sa.AreasId = a.Id AND a.AreaDatasetId = 19 
		                GROUP BY 
			                sa.SiteId
	                ) AS sapa ON s.Id = sapa.SiteId  
	                LEFT JOIN Area apa ON sapa.AreasId = apa.Id
                    { where }";

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
        public async Task<IEnumerable<SiteEntity>> GetAsync()
        {
            try
            {
                return await QueryAsync<SiteEntity>(GetSiteQuery("WHERE s.Id IN (SELECT SiteId FROM SearchableSightings)"));
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting sites");
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SiteEntity>> GetByIdsLiveAsync(IEnumerable<int> ids)
        {
            if (!ids?.Any() ?? true)
            {
                return null;
            }

            try
            {
                return await QueryAsync<SiteEntity>(GetSiteQuery(      $"WHERE s.Id IN ({ string.Join(',', ids) })--@siteIds"), null/*new { siteIds = ids }*/, true);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting sites by id");
                return null;
            }
        }
	}
}