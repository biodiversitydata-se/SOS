using Microsoft.Extensions.Logging;
using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Harvest.Services.Interfaces;

namespace SOS.Harvest.Repositories.Source.Artportalen
{
    /// <summary>
    ///     Area repository
    /// </summary>
    public class DiaryEntryRepository : BaseRepository<DiaryEntryRepository>, IDiaryEntryRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="artportalenDataService"></param>
        /// <param name="logger"></param>
        public DiaryEntryRepository(IArtportalenDataService artportalenDataService, ILogger<DiaryEntryRepository> logger) : base(
            artportalenDataService, logger)
        {
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DiaryEntryEntity>> GetAsync()
        {
            try
            {
                var query = @"
				SELECT 
					CloudinessId,
					ControlingOrganisationId,
					EndTime,
					StartDate AS IssueDate,
					OrganizationId,
					PrecipitationId,
					ProjectId,
					SiteId,
					SnowcoverId,
					StartTime,
					Temperature,
					UserId,
					VisibilityId,
					WindId,
					WindStrengthId
				FROM
				(
					SELECT 
						ROW_NUMBER() OVER(PARTITION BY 
							ControlingOrganisationId,
							EndTime,
							ProjectId,
							SiteId,
							StartDate,
							StartTime,
							UserId
						ORDER BY 
							StartDate DESC,
							StartTime DESC,
							EndTime DESC) 
						AS RowNumber,
						CloudinessId,
						ControlingOrganisationId,
						EndTime,
						OrganizationId,
						PrecipitationId,
						ProjectId,
						SiteId,
						SnowcoverId,
						StartDate,
						StartTime,
						t.[Value] AS Temperature,
						UserId,
						VisibilityId,
						WindId,
						WindStrengthId
					FROM 
						DiaryEntry de
						LEFT JOIN Temperature t ON de.TemperatureId = t.Id
					WHERE 
						IsPrivate = 0
						AND ProjectId IS NOT NULL
						AND UserId IS NOT NULL
						AND Startdate = EndDate
						AND (
							CloudinessId IS NOT NULL
							OR PrecipitationId IS NOT NULL
							OR SnowcoverId IS NOT NULL
							OR TemperatureId IS NOT NULL
							OR WindId IS NOT NULL
							OR WindStrengthId IS NOT NULL
							OR VisibilityId IS NOT NULL
						)
				) AS source
				WHERE 
					RowNumber = 1";

                return await QueryAsync<DiaryEntryEntity>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting diary entries");
                throw;
            }
        }
    }
}