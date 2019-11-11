using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SOS.Import.Entities;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Enums;

namespace SOS.Import.Repositories.Source.SpeciesPortal
{
    /// <summary>
    /// Area repository
    /// </summary>
    public class AreaRepository : BaseRepository<AreaRepository>, Interfaces.IAreaRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="SpeciesPortalDataService"></param>
        /// <param name="logger"></param>
        public AreaRepository(ISpeciesPortalDataService SpeciesPortalDataService, ILogger<AreaRepository> logger) : base(SpeciesPortalDataService, logger)
        {
           
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AreaEntity>> GetAsync()
        {
            try
            {
                var areaTypes = (int[]) Enum.GetValues(typeof(AreaType));

                var query = @"
                SELECT 
	                a.AreaDatasetId,
                    a.Id,
                    a.FeatureId,
                    a.Polygon,
	                a.Name,
                    a.ParentId
                FROM 
	                Area a
                WHERE
                    a.AreaDatasetId IN (" + string.Join(",", areaTypes) + ")";

                return await QueryAsync<AreaEntity>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting areas");
                return null;
            }
        }
    }
}
