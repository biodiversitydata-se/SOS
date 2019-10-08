using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SOS.Import.Entities;
using SOS.Import.Services.Interfaces;

namespace SOS.Import.Repositories.Source.SpeciesPortal
{
    /// <summary>
    /// Site repository
    /// </summary>
    public class SiteRepository : BaseRepository<SiteRepository>, Interfaces.ISiteRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="SpeciesPortalDataService"></param>
        /// <param name="logger"></param>
        public SiteRepository(ISpeciesPortalDataService SpeciesPortalDataService, ILogger<SiteRepository> logger) : base(SpeciesPortalDataService, logger)
        {
           
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SiteEntity>> GetAsync()
        {
            try
            {
               
                const string query = @"
                SELECT 
	                s.Id,
	                s.Name,
	                s.XCoord,
	                s.YCoord,
	                am.Id AS MunicipalityId,
	                am.Name AS MunicipalityName,
	                am.ParentId AS CountyId,
	                am.ParentAreaName AS CountyName,
	                ap.Id AS ProvinceId,
	                ap.Name AS ProvinceName,
	                acp.Id AS CountryPartId,
	                acp.Name AS CountryPartName
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
	                LEFT JOIN Area acp ON sacp.AreasId = acp.Id";

                return await QueryAsync<SiteEntity>(query);
            }
            catch(Exception e)
            {
                Logger.LogError(e, "Error getting sites");
                return null;
            }
            
        }
    }
}
