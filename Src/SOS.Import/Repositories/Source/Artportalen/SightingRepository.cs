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
using SOS.Lib.Repositories.Interfaces;

namespace SOS.Import.Repositories.Source.Artportalen
{
    public class SightingRepository : BaseRepository<SightingRepository>, ISightingRepository
    {
        private string GetSightingQuery(string where) => GetSightingQuery(0, null, where);

        private string GetSightingQuery(int top, string where) => GetSightingQuery(0, null, where);

        private string GetSightingQuery(string join, string where) => GetSightingQuery(0, join, where);

        private string SightingsFromBasics => @$"
            SearchableSightings s WITH(NOLOCK)
            INNER JOIN SightingState ss ON s.SightingId = ss.SightingId 
            {((ObservationType != ObservationType.Public) ? "INNER JOIN Taxon t ON s.TaxonId = t.Id": "")}";

        // Todo arguments for protected sightings
        private string SightingWhereBasics => @$" 
            s.SightingTypeId IN (0,3,8)
            AND s.SightingTypeSearchGroupId IN (1,16,32,256) 
	        AND s.ValidationStatusId <> 50
            AND ss.IsActive = 1
	        AND ss.SightingStateTypeId = 30 
            AND {((ObservationType != ObservationType.Public) ?
                @"(
                    (
                        s.ProtectedBySystem > 0 
                        AND t.ProtectionLevelId > 2
                    ) OR 
                    s.HiddenByProvider > GETDATE()
                )"
                :
                @"s.TaxonId IS NOT NULL"
            )}";
        
        /// <summary>
        /// Create sighting query
        /// </summary>
        /// <param name="top"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        private string GetSightingQuery(int top, string join, string where)
        {
            var topCount = "";

            if (top > 0)
            {
                topCount = $"TOP {top}";
            }

            var query = $@"
                SELECT DISTINCT {topCount} 
                    s.ActivityId,
                    s.DiscoveryMethodId,
					s.BiotopeId,
					sdb.[Description] AS BiotopeDescription,
                    ssci.Label AS CollectionID,
                    ssci.Id as SightingSpeciesCollectionItemId,
	                scp.Comment,
                    s.EditDate,
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
	                si.SiteId,
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
                    s.SightingTypeId,
                    s.SightingTypeSearchGroupId,
                    ssci.OrganizationId AS OrganizationCollectorId,
	                ssci.CollectorId AS UserCollectorId,
	                srDeterminer.UserId AS DeterminerUserId,
	                srDeterminer.DeterminationYear AS DeterminationYear,
	                srConfirmator.UserId AS ConfirmatorUserId,
	                srConfirmator.DeterminationYear AS ConfirmationYear,
	                svr.RegionalSightingStateId as RegionalSightingStateId,
                    (select string_agg(SightingPublishTypeId, ',') from SightingPublish sp where SightingId = s.SightingId group by SightingId) AS SightingPublishTypeIds,
                    (select string_agg(SpeciesFactId , ',') from SpeciesFactTaxon sft where sft.TaxonId = s.TaxonId group by sft.TaxonId) AS SpeciesFactsIds
                FROM
	                {SightingsFromBasics}
                    {join}
					INNER JOIN Sighting si ON s.SightingId = si.Id
	                LEFT JOIN SightingCommentPublic scp ON s.SightingId = scp.SightingId
	                LEFT JOIN SightingSpeciesCollectionItem ssci ON s.SightingId = ssci.SightingId
	                LEFT JOIN SightingBarcode sb ON s.SightingId = sb.SightingId
                    LEFT JOIN [User] u ON s.OwnerUserId = u.Id 
	                LEFT JOIN Person p ON u.PersonId = p.Id
                    LEFT JOIN MigrateSightingid msi ON s.SightingId = msi.Id
					LEFT JOIN SightingDescription sdb ON si.SightingBiotopeDescriptionId = sdb.Id 
					LEFT JOIN SightingDescription sds ON si.SightingSubstrateDescriptionId = sds.Id 
					LEFT JOIN SightingDescription sdss ON si.SightingSubstrateSpeciesDescriptionId = sdss.Id
                    LEFT JOIN SightingRelation srDeterminer ON srDeterminer.SightingId = s.SightingId AND srDeterminer.IsPublic = 1 AND srDeterminer.SightingRelationTypeId = 3
                    LEFT JOIN SightingRelation srConfirmator ON srConfirmator.SightingId = s.SightingId AND srConfirmator.IsPublic = 1 AND srConfirmator.SightingRelationTypeId = 5
                    LEFT JOIN TriggeredValidationRule tvr on tvr.SightingId = ss.SightingId
                    LEFT JOIN StatusValidationRule svr on svr.Id = tvr.StatusValidationRuleId                    
                WHERE
	                {SightingWhereBasics}
                    {where} ";

            return query;
        }
            

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
        public async Task<IEnumerable<SightingEntity>> GetChunkAsync(int startId, int maxRows)
        {
            try
            {
                var query = GetSightingQuery("AND s.SightingId BETWEEN @StartId AND @EndId");

                return await QueryAsync<SightingEntity>(query, new {StartId = startId, EndId = startId + maxRows - 1}, Live);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting sightings");

                return null;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SightingEntity>> GetChunkAsync(IEnumerable<int> sightingIds)
        {
            try
            {
                var query = GetSightingQuery("INNER JOIN @tvp t ON s.SightingId = t.Id", null);

                return await QueryAsync<SightingEntity>(query, new { tvp = sightingIds.ToDataTable().AsTableValuedParameter("dbo.IdValueTable") }, Live);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting sightings");

                return null;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SightingEntity>> GetChunkAsync(DateTime modifiedSince, int maxRows)
        {
            try
            {
                var query = GetSightingQuery(maxRows, "AND s.EditDate > @modifiedSince");

                return await QueryAsync<SightingEntity>(query, new { modifiedSince = modifiedSince.ToLocalTime() }, Live);
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
                string query = $@"
                SELECT 
                    MIN(s.SightingId) AS Item1,
                    MAX(s.SightingId) AS Item2
		        FROM 
		            {SightingsFromBasics}
                WHERE 
                    {SightingWhereBasics}";

                return (await QueryAsync<Tuple<int, int>>(query, null, Live)).FirstOrDefault();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting min and max id");

                return null;
            }
        }

        /// <inheritdoc />
        public async Task<DateTime?> GetLastModifiedDateAsyc()
        {
            try
            {
                string query = $@"
                SELECT 
	                MAX(s.EditDate)
                FROM 
	               {SightingsFromBasics}
                WHERE
                    {SightingWhereBasics}";

                return (await QueryAsync<DateTime?>(query, null, Live)).FirstOrDefault();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting last modified date");

                return null;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<int>> GetModifiedIdsAsync(DateTime modifiedSince, int limit)
        {
            try
            {
                var query = $@"SELECT TOP({limit})   
	               s.SightingId AS Id
                FROM
	                {SightingsFromBasics}
                WHERE
	                {SightingWhereBasics}
                    AND s.EditDate > @modifiedSince
                ORDER BY
                    s.EditDate";

                return await QueryAsync<int>(query, new { modifiedSince = modifiedSince.ToLocalTime() }, Live);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting modified id's");

                return null;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<(int SightingId, int ProjectId)>> GetSightingProjectIdsAsync(IEnumerable<int> sightingIds)
        {
            try
            {
                string query = $@"
                SELECT 
                    ps.SightingId AS SightingId,
	                ps.ProjectId AS ProjectId
                FROM 
	                ProjectSighting ps
                    INNER JOIN @tvp t ON ps.SightingId = t.Id 
                ORDER BY 
                    ps.ProjectId DESC";

                return await QueryAsync<(int SightingId, int ProjectId)>(
                    query,
                    new { tvp = sightingIds.ToDataTable().AsTableValuedParameter("dbo.IdValueTable") }, Live);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting sighting/project connections");
                return null;
            }
        }

        /// <inheritdoc />
        public bool Live { get; set; }

        /// <inheritdoc />
        public ObservationType ObservationType{ get; set; }
    }
}