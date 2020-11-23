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
                    { join } 
	                LEFT JOIN -- Bad data exists, some sites are connected to more than one Municipality
	                (
		                 SELECT
			                sa.SiteId,
			                sa.AreasId,
							ROW_NUMBER() OVER (PARTITION BY sa.SiteId ORDER BY sa.AreasId) AS MunicipalityIndex
		                FROM
			                SiteAreas sa
			                INNER JOIN Area a ON sa.AreasId = a.Id AND a.AreaDatasetId = 1 
	                ) AS sam ON s.Id = sam.SiteId AND sam.MunicipalityIndex = 1
	                LEFT JOIN Area am ON sam.AreasId = am.Id
	                LEFT JOIN -- Bad data exists, some sites are connected to more than one province
	                (
		                SELECT
			                sa.SiteId,
			                sa.AreasId,
							ROW_NUMBER() OVER (PARTITION BY sa.SiteId ORDER BY sa.AreasId) AS ProvinceIndex
		                FROM
			                SiteAreas sa
			                INNER JOIN Area a ON sa.AreasId = a.Id AND a.AreaDatasetId = 16 
	                ) AS sap ON s.Id = sap.SiteId AND sap.ProvinceIndex = 1
	                LEFT JOIN Area ap ON sap.AreasId = ap.Id
	                LEFT JOIN -- Bad data exists, some sites are connected to more than one country part
	                (
		                SELECT
			                sa.SiteId,
			                sa.AreasId,
							ROW_NUMBER() OVER (PARTITION BY sa.SiteId ORDER BY sa.AreasId) AS CountyIndex
		                FROM
			                SiteAreas sa
			                INNER JOIN Area a ON sa.AreasId = a.Id AND a.AreaDatasetId = 13 
	                ) AS sacp ON s.Id = sacp.SiteId AND sacp.CountyIndex = 1
	                LEFT JOIN Area acp ON sacp.AreasId = acp.Id
	                LEFT JOIN -- Bad data exists, some sites are connected to more than one parish
	                (
		                SELECT
			                sa.SiteId,
			                sa.AreasId,
							ROW_NUMBER() OVER (PARTITION BY sa.SiteId ORDER BY sa.AreasId) AS ParishIndex
						FROM
			                SiteAreas sa
			                INNER JOIN Area a ON sa.AreasId = a.Id AND a.AreaDatasetId = 19 
	                ) AS sapa ON s.Id = sapa.SiteId AND sapa.ParishIndex = 1
	                LEFT JOIN Area apa ON sapa.AreasId = apa.Id";

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
               

				return await QueryAsync<SiteEntity>(GetSiteQuery("INNER JOIN SearchableSightings ss ON s.Id = ss.SiteId"));
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting sites");
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

            try
            {
                return await QueryAsync<SiteEntity>(GetSiteQuery(      
                        $"INNER JOIN @tvp t ON s.Id = t.Id"), 
                    new { tvp = ids.ToDataTable().AsTableValuedParameter("dbo.IdValueTable") }, live);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting sites by id");
                return null;
            }
        }
	}
}