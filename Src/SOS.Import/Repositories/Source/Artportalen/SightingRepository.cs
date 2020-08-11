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
    public class SightingRepository : BaseRepository<SightingRepository>, ISightingRepository
    {
        /// <summary>
		/// Create sighting query
		/// </summary>
		/// <param name="where"></param>
		/// <returns></returns>
        private string GetSightingQuery(string where) =>
            $@"
                SELECT DISTINCT
                    s.ActivityId,
                    s.DiscoveryMethodId,
					s.BiotopeId,
					sdb.[Description] AS BiotopeDescription,
                    ssci.Label AS CollectionID,
                    ssci.Id as SightingSpeciesCollectionItemId,
	                scp.Comment,
	                s.EndDate,
	                s.EndTime,
	                s.GenderId,
                    s.HasImages,
	                s.HiddenByProvider,
	                s.SightingId AS Id, 
	                ssci.Label,
	                s.[Length],
                    s.MaxDepth,
					s.MaxHeight,
					s.MinDepth,
					s.MinHeight,
	                s.NotPresent,
	                s.NotRecovered,
                    s.OwnerOrganizationId,
                    msi.PortalId AS MigrateSightingPortalId,
                    msi.obsid AS MigrateSightingObsId,
	                s.ProtectedBySystem,
	                s.Quantity,
					s.QuantityOfSubstrate,
                    s.RegisterDate,
	                CASE 
						WHEN p.Id IS NULL THEN null
						ELSE p.FirstName + ' ' + p.LastName 
					END AS RightsHolder,
	                s.SiteId,
	                s.StageId,
	                s.StartDate,
	                s.StartTime,
					s.SubstrateId,
					sds.[Description] AS SubstrateDescription,
					sdss.[Description] AS SubstrateSpeciesDescription,
					s.SubstrateSpeciesId,
	                s.TaxonId,
	                s.UnsureDetermination,
	                s.Unspontaneous,
	                s.UnitId,
	                sb.URL,
                    s.ValidationStatusId,
	                s.[Weight], 
	                s.HasTriggeredValidationRules, 
	                s.HasAnyTriggeredValidationRuleWithWarning,
	                s.NoteOfInterest,
	                si.DeterminationMethodId,
	                site.ExternalId AS SiteExternalId,
                    s.SightingTypeId,
                    s.SightingTypeSearchGroupId,
                    ssci.OrganizationId AS OrganizationCollectorId,
	                ssci.CollectorId AS UserCollectorId,
	                srDeterminer.UserId AS DeterminerUserId,
	                srDeterminer.DeterminationYear AS DeterminationYear,
	                srConfirmator.UserId AS ConfirmatorUserId,
	                srConfirmator.DeterminationYear AS ConfirmationYear,
	                svr.RegionalSightingStateId as RegionalSightingStateId,
                    (select string_agg(SightingPublishTypeId, ',') from SightingPublish sp where SightingId = s.SightingId group by SightingId) SightingPublishTypeIds
                FROM
	                SearchableSightings s WITH(NOLOCK)
					INNER JOIN Sighting si ON s.SightingId = si.Id
	                INNER JOIN SightingState ss ON s.SightingId = ss.SightingId
	                LEFT JOIN SightingCommentPublic scp ON s.SightingId = scp.SightingId
	                LEFT JOIN SightingSpeciesCollectionItem ssci ON s.SightingId = ssci.SightingId
	                LEFT JOIN SightingBarcode sb ON s.SightingId = sb.SightingId
                    LEFT JOIN [User] u ON s.OwnerUserId = u.Id 
	                LEFT JOIN Person p ON u.PersonId = p.Id
                    LEFT JOIN MigrateSightingid msi ON s.SightingId = msi.Id
					LEFT JOIN SightingDescription sdb ON si.SightingBiotopeDescriptionId = sdb.Id 
					LEFT JOIN SightingDescription sds ON si.SightingSubstrateDescriptionId = sds.Id 
					LEFT JOIN SightingDescription sdss ON si.SightingSubstrateSpeciesDescriptionId = sdss.Id
                    LEFT JOIN Site site on site.Id = s.SiteId 
                    LEFT JOIN SightingRelation srDeterminer ON srDeterminer.SightingId = s.SightingId AND srDeterminer.IsPublic = 1 AND srDeterminer.SightingRelationTypeId = 3
                    LEFT JOIN SightingRelation srConfirmator ON srConfirmator.SightingId = s.SightingId AND srConfirmator.IsPublic = 1 AND srConfirmator.SightingRelationTypeId = 5
                    LEFT JOIN TriggeredValidationRule tvr on tvr.SightingId = ss.SightingId
                    LEFT JOIN StatusValidationRule svr on svr.Id = tvr.StatusValidationRuleId                    
                WHERE
	                { where }
	                AND s.TaxonId IS NOT NULL	 
                    AND s.SightingTypeId IN(0, 3) 
	                AND s.HiddenByProvider IS NULL
	                AND s.ValidationStatusId NOT IN(50)	   
                    AND s.SightingTypeSearchGroupId & 33 > 0
	                AND ss.IsActive = 1
	                AND ss.SightingStateTypeId = 30 --Published
	                AND (ss.EndDate IS NULL OR ss.EndDate > GETDATE()) ";

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="artportalenDataService"></param>
        /// <param name="logger"></param>
        public SightingRepository(IArtportalenDataService artportalenDataService, ILogger<SightingRepository> logger) :
            base(artportalenDataService, logger)
        {
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SightingEntity>> GetChunkAsync(int startId, int maxRows, bool liveData)
        {
            try
            {
                var query = GetSightingQuery("s.SightingId BETWEEN @StartId AND @EndId");

                return await QueryAsync<SightingEntity>(query, new {StartId = startId, EndId = startId + maxRows - 1}, liveData);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting sightings");

                return null;
            }
        }

        /// <summary>
        ///     Get sightings for the specified sighting ids. Used for testing purpose for retrieving specific sightings from
        ///     Artportalen.
        ///     This method should be the same as GetChunkAsync(int startId, int maxRows), with
        ///     the difference that this method uses a list of sighting ids instead of (startId, maxRows).
        /// </summary>
        public async Task<IEnumerable<SightingEntity>> GetChunkAsync(IEnumerable<int> sightingIds)
        {
            try
            {
                var query = GetSightingQuery("s.SightingId in @ids");

                return await QueryAsync<SightingEntity>(query, new {ids = sightingIds});
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting sightings");

                return null;
            }
        }

        /// <inheritdoc />
        public async Task<Tuple<int, int>> GetIdSpanAsync()
        {
            try
            {
                const string query = @"
                SELECT 
                    MIN(SightingId) AS Item1,
                    MAX(SightingId) AS Item2
		        FROM 
		            SearchableSightings s";

                return (await QueryAsync<Tuple<int, int>>(query)).FirstOrDefault();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting min and max id");

                return null;
            }
        }

        /// <inheritdoc />
        public async Task<int> GetMaxIdLiveAsync()
        {
            try
            {
                const string query = @"
                SELECT 
                    MAX(SightingId) 
		        FROM 
		            SearchableSightings";

                return (await QueryAsync<int>(query, null, true)).FirstOrDefault();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error live max id");

                return 0;
            }
        }

        /// <inheritdoc />
        public async Task<DateTime?> GetLastModifiedDateAsyc()
        {
            try
            {
                const string query = @"
                SELECT 
	                MAX(EditDate)
                FROM 
	                Sighting";

                return (await QueryAsync<DateTime?>(query)).FirstOrDefault();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting last modified date");

                return null;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<(int SightingId, int ProjectId)>> GetProjectIdsAsync()
        {
            try
            {
                const string query = @"
                SELECT 
                    ps.SightingId AS SightingId,
	                ps.ProjectId AS ProjectId
                FROM 
	                ProjectSighting ps
                ORDER BY ps.ProjectId DESC";

                return await QueryAsync<(int SightingId, int ProjectId)>(query);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting sighting/project connections");
                return null;
            }
        }
    }
}