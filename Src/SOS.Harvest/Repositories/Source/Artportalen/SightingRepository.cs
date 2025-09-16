﻿using Dapper;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Extensions;

namespace SOS.Harvest.Repositories.Source.Artportalen
{
    public class SightingRepository : BaseRepository<ISightingRepository>, ISightingRepository
    {
        private string GetSightingQuery(int top, string where) => GetSightingQuery(top, null!, where);


        /// <summary>
        /// Create sighting query
        /// </summary>
        /// <param name="top"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        private string GetSightingQuery(int top, string join, string where)
        {
            var topCount = "";

            // Adding TOP increases the performance
            if (top > 0)
            {
                topCount = $"TOP {top}";
            }

            var triggerRuleSelect = "svr.RegionalSightingStateId";
            var triggerRuleFrom = @"LEFT JOIN TriggeredValidationRule tvr on tvr.SightingId = si.Id 
                    LEFT JOIN StatusValidationRule svr ON svr.Id = tvr.StatusValidationRuleId ";

            if (DataService.Configuration?.UseTriggeredObservationRule ?? false)
            {
                triggerRuleSelect = @"tor.ActivityRuleId AS TriggeredObservationRuleActivityRuleId,
                tor.FrequencyId AS TriggeredObservationRuleFrequencyId,
                tor.PeriodRuleId AS TriggeredObservationRulePeriodRuleId,
                tor.PromptRuleId AS TriggeredObservationRulePromptRuleId,
                CASE WHEN tor.Prompts IS NULL THEN NULL WHEN LEN(tor.Prompts) = 0 THEN 0 ELSE 1 END AS TriggeredObservationRulePrompts,
                tor.RegionalSightingState AS TriggeredObservationRuleRegionalSightingState,
                tor.ReproductionId AS TriggeredObservationRuleReproductionId,
                tor.StatusRuleId AS TriggeredObservationRuleStatusRuleId,
                tor.Unspontaneous AS TriggeredObservationRuleUnspontaneous";
                //ISNULL(tor.RegionalSightingState, 0) AS TriggeredObservationRuleRegionalSightingState";
                triggerRuleFrom = @"LEFT JOIN TriggeredObservationRule tor ON tor.SightingId = si.Id ";
            }

            var query = $@"
                SELECT {topCount} 
                    si.Id, 
                    s.ActivityId,
                    s.ChecklistId,
                    s.DiscoveryMethodId,
					s.BiotopeId,
					sdb.[Description] AS BiotopeDescription,
	                scp.Comment,
                    s.EditDate,
	                s.EndDate,
	                s.EndTime,
	                s.GenderId,
                    s.HasImages,
	                s.HiddenByProvider,
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
	                CASE WHEN p.Id IS NULL THEN null ELSE p.FirstName + ' ' + p.LastName END AS RightsHolder,
                    si.SiteId,
                    CASE WHEN sic.SightingId IS NULL THEN 0 ELSE 1 END AS HasUserComments,
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
	                sb.URL AS SightingBarcodeURL,
                    s.ValidationStatusId,
	                s.[Weight], 
	                s.HasTriggeredValidationRules, 
	                s.HasAnyTriggeredValidationRuleWithWarning,
	                s.NoteOfInterest,
	                si.DeterminationMethodId,
                    s.SightingTypeId,
                    s.SightingTypeSearchGroupId,
                    s.FieldDiaryGroupId,
                    ssu.Summary,
                    ISNULL(ssu.FreeTextSummary, 0) AS IsFreeTextSummary,
	                {triggerRuleSelect},
                    (select string_agg(SightingPublishTypeId, ',') from SightingPublish sp where SightingId = si.Id group by SightingId) AS SightingPublishTypeIds,
                    (select string_agg(SpeciesFactId , ',') from SpeciesFactTaxon sft where sft.TaxonId = s.TaxonId AND sft.IsSearchFilter = 1 group by sft.TaxonId) AS SpeciesFactsIds,
                    sdc.DatasourceId
                FROM
	                {SightingsFromBasics}
					INNER JOIN Sighting si ON si.Id = s.SightingId
                    {join}
	                LEFT JOIN SightingCommentPublic scp ON scp.SightingId = si.Id                                   
	                LEFT JOIN SightingBarcode sb ON sb.SightingId = si.Id
                    LEFT JOIN [User] u ON u.Id = s.OwnerUserId  
	                LEFT JOIN Person p ON p.Id = u.PersonId 
                    LEFT JOIN MigrateSightingid msi ON msi.Id = si.Id 
					LEFT JOIN SightingDescription sdb ON sdb.Id = si.SightingBiotopeDescriptionId
					LEFT JOIN SightingDescription sds ON sds.Id = si.SightingSubstrateDescriptionId 
					LEFT JOIN SightingDescription sdss ON sdss.Id = si.SightingSubstrateSpeciesDescriptionId
                    LEFT JOIN SightingSummary ssu ON ssu.Id = si.SightingSummaryId
                    {triggerRuleFrom}
                    LEFT JOIN SightingDatasource sdc ON sdc.SightingId = si.Id 
                    LEFT JOIN (SELECT SightingId FROM SightingComment GROUP BY SightingId) sic ON sic.SightingId = si.Id 
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
        public async Task<IEnumerable<SightingEntity>?> GetChunkAsync(int startId, int maxRows)
        {
            try
            {
                var query = GetSightingQuery(0, "AND si.Id BETWEEN @StartId AND @EndId");

                var result = (await QueryAsync<SightingEntity>(query, new { StartId = startId, EndId = startId + maxRows - 1 }))?.ToArray();
                if ((result?.Count() ?? 0) == 0)
                {
                    Logger.LogDebug($"Artportalen SightingRepository.GetChunkAsync returned no sightings. Live={Live}, startId={startId}, maxRows={maxRows}");
                }

                return result?.DistinctBy(s => s.Id);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting sightings");

                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SightingEntity>?> GetChunkAsync(IEnumerable<int> sightingIds)
        {
            try
            {
                if (!sightingIds?.Any() ?? true)
                {
                    return null;
                }
                var query = GetSightingQuery(sightingIds?.Count() ?? 0, "INNER JOIN @tvp t ON si.Id = t.Id", null!);

                var result = (await QueryAsync<SightingEntity>(query, new { tvp = sightingIds.ToSqlRecords().AsTableValuedParameter("dbo.IdValueTable") }))?.ToArray();
                if ((result?.Count() ?? 0) == 0)
                {
                    Logger.LogInformation($"Artportalen SightingRepository.GetChunkAsync returned no sightings. Live={Live}, sightingIds.Count()={sightingIds!.Count()}.\n,The first five SightingIds used in @tvp are: {string.Join(", ", sightingIds!.Take(5))}");
                    Logger.LogDebug(query);
                }

                return result;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting sightings");

                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SightingEntity>?> GetChunkAsync(DateTime modifiedSince, int maxRows)
        {
            try
            {
                var query = GetSightingQuery(maxRows, "AND s.EditDate > @modifiedSince");

                var result = (await QueryAsync<SightingEntity>(query, new { modifiedSince = modifiedSince.ToLocalTime() }))?.ToArray();
                if (!result?.Any() ?? true)
                {
                    Logger.LogDebug($"Artportalen SightingRepository.GetChunkAsync returned no sightings. Live={Live}, modifiedSince={modifiedSince}, maxRows={maxRows}");
                }

                return result?.DistinctBy(s => s.Id);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting sightings");

                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<int>> GetDeletedIdsAsync(DateTime from)
        {
            string query = @$"
            SELECT 
	            st.SightingId 
            FROM 
	            SightingState st
            WHERE 
	            st.StartDate > '{from.ToLocalTime().ToString("yyyy-MM-dd hh:mm")}'
	            AND st.SightingStateTypeId = 50
	            AND st.isactive = 1
            UNION
			SELECT 
				TemporaryRemovedSightingId
			FROM 
				TemporaryRemovedSightingIds";

            return await QueryAsync<int>(query, null!);
        }

        public async Task<IEnumerable<int>> GetRejectedIdsAsync(DateTime modifiedSince)
        {
            string query = @$"
            SELECT 
	            s.Id 
            FROM 
	            Sighting s
            WHERE 
                s.ValidationStatusId = 50
	            AND s.EditDate > '{modifiedSince.ToLocalTime().ToString("yyyy-MM-dd hh:mm")}'";

            return await QueryAsync<int>(query, null!);
        }

        /// <inheritdoc />
        public async Task<(int minId, int maxId)> GetIdSpanAsync()
        {
            try
            {
                string query = $@"
                SELECT 
                    MIN(s.SightingId) AS minId,
                    MAX(s.SightingId) AS maxId
		        FROM 
		            {SightingsFromBasics}
                WHERE 
                    {SightingWhereBasics}";

                return (await QueryAsync<(int minId, int maxId)>(query, null!)).FirstOrDefault();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting min and max id");

                throw;
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

                return (await QueryAsync<DateTime?>(query, null!)).FirstOrDefault();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting last modified date");

                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<NewAndEditedSightingId>> GetModifiedIdsAsync(DateTime modifiedSince, int limit)
        {
            try
            {
                var result = await QueryAsync<NewAndEditedSightingId>("GetNewAndEditedSightingIds", new { modifiedSince = modifiedSince.ToLocalTime(), maxReturnRows = limit }, System.Data.CommandType.StoredProcedure);

                Logger.LogInformation($"GetModifiedIdsAsync({modifiedSince.ToLocalTime()}, {limit}, Live={Live}) returned {result?.Count() ?? 0} sightingIds.");
                if (!result?.Any() ?? true)
                {
                    Logger.LogInformation($"Artportalen SightingRepository.GetModifiedIdsAsync(DateTime modifiedSince, int limit) returned no sightings. modifiedSince={modifiedSince}, modifiedSinceLocalTime={modifiedSince.ToLocalTime()}, limit={limit}");
                    return null!;
                }
                return result!.DistinctBy(m => m.Id);                
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting modified id's");

                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<(int SightingId, int ProjectId)>> GetSightingProjectIdsAsync(IEnumerable<int> sightingIds)
        {
            try
            {
                var query = $@"
                SELECT 
                    ps.SightingId AS SightingId,
	                ps.ProjectId AS ProjectId
                FROM 
	                ProjectSighting ps 
                    INNER JOIN @tvp t ON ps.SightingId = t.Id";

                return await QueryAsync<(int SightingId, int ProjectId)>(
                    query,
                    new { tvp = sightingIds.ToSqlRecords().AsTableValuedParameter("dbo.IdValueTable") });
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting sighting/project connections");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IDictionary<int, ICollection<(int sightingId, int taxonId)>>> GetSightingsAndTaxonIdsForChecklistsAsync(IEnumerable<int> checklistIds)
        {
            var query = $@"
	        SELECT 
		        s.ChecklistId,
		        s.[Id] AS SightingId,
		        s.[TaxonId]
	        FROM 
		        Sighting s
                INNER JOIN @tvp t ON s.ChecklistId = t.Id";

            var result = await QueryAsync<(int checklistId, int sightingId, int taxonId)>(query,
                new { tvp = checklistIds.ToSqlRecords().AsTableValuedParameter("dbo.IdValueTable") });

            if (!result?.Any() ?? true)
            {
                return null!;
            }

            var checklistsData = new Dictionary<int, ICollection<(int sightingId, int taxonId)>>();

            foreach (var item in result!)
            {
                if (!checklistsData.TryGetValue(item.checklistId, out var checklistData))
                {
                    checklistData = new List<(int sightingId, int taxonId)>();
                    checklistsData.Add(item.checklistId, checklistData);
                }
                checklistData.Add((item.sightingId, item.taxonId));
            }

            return checklistsData;
        }
    }
}