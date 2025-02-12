using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Exceptions;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Cache;
using SOS.Lib.Models.Log;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Extensions.Dto;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using SOS.Lib.Models.Search.Enums;

namespace SOS.Observations.Api.Managers
{
    /// <summary>
    ///     Observation manager class
    /// </summary>
    public class ObservationManager : IObservationManager
    {
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly IProtectedLogRepository _protectedLogRepository;
        private readonly IFilterManager _filterManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IVocabularyValueResolver _vocabularyValueResolver;
        private readonly ITaxonObservationCountCache _taxonObservationCountCache;
        private readonly IClassCache<Dictionary<int, TaxonSumAggregationItem>> _taxonSumAggregationCache;
        private readonly IGeneralizationResolver _generalizationResolver;
        private readonly IDataProviderCache _dataProviderCache;
        private readonly ILogger<ObservationManager> _logger;

        private async Task<long> GetProvinceCountAsync(int? roleId, string authorizationApplicationIdentifier, SearchFilterBase filter)
        {
            await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);
            return await _processedObservationRepository.GetProvinceCountAsync(filter);
        }

        private async Task PostProcessObservations(SearchFilter filter, ProtectionFilter protectionFilter, IEnumerable<dynamic> processedObservations, string cultureCode)
        {
            if (!processedObservations?.Any() ?? true)
            {
                return;
            }

            try
            {
                var occurenceIds = new HashSet<string>();
                var observations = processedObservations.Cast<IDictionary<string, object>>().ToList();
                LocalDateTimeConverterHelper.ConvertToLocalTime(observations);

                // Resolve vocabulary values.
                _vocabularyValueResolver.ResolveVocabularyMappedValues(observations, cultureCode);

                foreach (var obs in observations)
                {
                    if (!protectionFilter.Equals(ProtectionFilter.Public) &&
                        obs.TryGetValue(nameof(Observation.Sensitive).ToLower(), out var sensitive) &&
                        obs.TryGetValue(nameof(Observation.Occurrence).ToLower(), out var occurrenceObject)
                    )
                    {
                        if (((bool)sensitive) && occurrenceObject is IDictionary<string, object> occurrenceDictionary && occurrenceDictionary.TryGetValue("occurrenceId", out var occurenceId))
                        {
                            occurenceIds.Add(occurenceId as string);
                        }
                    }
                }

                // Log protected observations
                if (!protectionFilter.Equals(ProtectionFilter.Public))
                {
                    var user = _httpContextAccessor.HttpContext?.User;

                    var protectedLog = new ProtectedLog
                    {
                        ApplicationIdentifier = user.Claims.Where(c =>
                            c.Type.Equals("client_id",
                                StringComparison.CurrentCultureIgnoreCase)).Select(c => c.Value).FirstOrDefault(),
                        Ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                        IssueDate = DateTime.Now,
                        User = user.Claims.Where(c =>
                            c.Type.Equals("name",
                                StringComparison.CurrentCultureIgnoreCase)).Select(c => c.Value).FirstOrDefault(),
                        UserId = user.Claims.Where(c =>
                            c.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                                StringComparison.CurrentCultureIgnoreCase)).Select(c => c.Value).FirstOrDefault(),
                        OccurenceIds = occurenceIds
                    };
                    await _protectedLogRepository.AddAsync(protectedLog);
                }

                await _generalizationResolver.ResolveGeneralizedObservationsAsync(filter, observations);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in postprocessing observations");
                throw;
            }
        }

        private async Task ResolveGeneralizedObservations(SearchFilter filter, List<IDictionary<string, object>> observations)
        {
            try
            {
                List<IDictionary<string, object>> generalizedObservations = GetGeneralizedObservations(observations);
                var generalizedOccurrenceIds = GetOccurrenceIds(generalizedObservations);
                var protectedFilter = filter.Clone();
                protectedFilter.ExtendedAuthorization.ProtectionFilter = ProtectionFilter.Sensitive;
                protectedFilter.IncludeSensitiveGeneralizedObservations = true;
                protectedFilter.IsPublicGeneralizedObservation = false;
                protectedFilter.OccurrenceIds = generalizedOccurrenceIds;
                if (protectedFilter.Output.Fields != null)
                {
                    if (!protectedFilter.Output.Fields.Contains("Occurrence.OccurrenceId"))
                    {
                        protectedFilter.Output.Fields.Add("Occurrence.OccurrenceId");
                    }
                    if (!protectedFilter.Output.Fields.Contains("IsGeneralized"))
                    {
                        protectedFilter.Output.Fields.Add("IsGeneralized");
                    }
                }

                await _filterManager.PrepareFilterAsync(null, null, protectedFilter); // todo - add role and applicationId
                var dynamicSensitiveObservationsResult = await _processedObservationRepository.GetChunkAsync(protectedFilter, 0, 1000);

                List<IDictionary<string, object>> sensitiveObservations = dynamicSensitiveObservationsResult.Records.Cast<IDictionary<string, object>>().ToList();
                UpdateGeneralizedWithRealValues(generalizedObservations, sensitiveObservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when resolving generalized observations");
            }
        }

        private void UpdateGeneralizedWithRealValues(List<IDictionary<string, object>> observations, List<IDictionary<string, object>> realObservations)
        {
            var obsByOccurrenceId = CreateObservationsByOccurrenceId(observations);
            var realObsByOccurrenceId = CreateObservationsByOccurrenceId(realObservations);
            int nrUpdated = 0;
            foreach (var kvp in realObsByOccurrenceId)
            {
                if (obsByOccurrenceId.TryGetValue(kvp.Key, out var observation))
                {
                    UpdateGeneralizedWithRealValues(observation, kvp.Value);
                    nrUpdated++;
                }
            }

            _logger.LogInformation($"{nrUpdated} generalized observations were updated with real coordinates.");
        }

        private Dictionary<string, IDictionary<string, object>> CreateObservationsByOccurrenceId(List<IDictionary<string, object>> observations)
        {
            var dict = new Dictionary<string, IDictionary<string, object>>();
            foreach(var obs in observations)
            {
                string? occurrenceId = GetValue<string?>(obs, "occurrence.occurrenceId");
                if (occurrenceId != null)
                {
                    dict.Add(occurrenceId, obs);
                }
            }

            return dict;
        }

        private T? GetValue<T>(IDictionary<string, object> obs, string propertyPath)
        {
            var parts = propertyPath
                .Split(".")
                .Select(m => m.ToCamelCase());

            var currentVal = obs;
            foreach (var part in parts)
            {
                if (currentVal.TryGetValue(part, out var currentValObject))
                {
                    if (currentValObject is IDictionary<string, object>)
                    {
                        currentVal = (IDictionary<string, object>)currentValObject;
                    }
                    else
                    {
                        return (T)currentValObject;
                    }
                }
            }

            return default;
        }

        private void UpdateValue<T>(IDictionary<string, object> obs, string propertyPath, T newValue)
        {
            var parts = propertyPath
                .Split(".")
                .Select(m => m.ToCamelCase())
                .ToList();

            var currentVal = obs;
            for (int i = 0; i < parts.Count; i++)
            {
                string part = parts[i];
                if (i == parts.Count - 1)
                {
                    if (currentVal.ContainsKey(part))
                    {
                        currentVal[part] = newValue;
                    }
                    return;
                }

                if (currentVal.TryGetValue(part, out var currentValObject))
                {
                    if (currentValObject is IDictionary<string, object>)
                    {
                        currentVal = (IDictionary<string, object>)currentValObject;
                    }
                }
            }
        }

        private void UpdateGeneralizedWithRealValues(IDictionary<string, object> obs, IDictionary<string, object> realObs)
        {
            // isGeneralized
            if (obs.ContainsKey("isGeneralized"))
            {
                if (realObs.ContainsKey("isGeneralized"))
                {
                    obs["isGeneralized"] = realObs["isGeneralized"];
                }
            }

            if (obs.ContainsKey("location"))
            {
                if (realObs.ContainsKey("location"))
                {
                    obs["location"] = realObs["location"];
                }
            }

            // Replace all fields
            //foreach (var key in obs.Keys)
            //{
            //    if (realObs.ContainsKey(key))
            //    {
            //        obs[key] = realObs[key];
            //    }
            //}
        }

        private List<string> GetOccurrenceIds(List<IDictionary<string, object>> observations)
        {
            var occurrenceIds = new List<string>();
            foreach (var obs in observations)
            {
                // Occurrence
                if (obs.TryGetValue(nameof(Observation.Occurrence).ToLower(),
                                out var occurrenceObject))
                {
                    var occurrenceDictionary = occurrenceObject as IDictionary<string, object>;
                    if (occurrenceDictionary.TryGetValue("occurrenceId", out var occurrenceId))
                    {
                        occurrenceIds.Add((string)occurrenceId);
                    }
                }
            }

            return occurrenceIds;
        }

        private List<IDictionary<string, object>> GetGeneralizedObservations(List<IDictionary<string, object>> observations)
        {
            var generalizedObservations = new List<IDictionary<string, object>>();
            try
            {
                foreach (var obs in observations)
                {
                    if (obs == null) continue;
                    if (obs.ContainsKey("isGeneralized"))
                    {
                        bool isGeneralized = (bool)obs["isGeneralized"];
                        if (isGeneralized)
                        {
                            generalizedObservations.Add(obs);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when getting generalized observations");
            }

            return generalizedObservations;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="protectedLogRepository"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="filterManager"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="taxonObservationCountCache"></param>
        /// <param name="taxonSumAggregationCache"></param>
        /// <param name="generalizationResolver"></param>
        /// <param name="dataProviderCache"></param>
        /// <param name="logger"></param>
        public ObservationManager(
            IProcessedObservationRepository processedObservationRepository,
            IProtectedLogRepository protectedLogRepository,
            IVocabularyValueResolver vocabularyValueResolver,
            IFilterManager filterManager,
            IHttpContextAccessor httpContextAccessor,
            ITaxonObservationCountCache taxonObservationCountCache,
            IClassCache<Dictionary<int, TaxonSumAggregationItem>> taxonSumAggregationCache,
            IGeneralizationResolver generalizationResolver,
            IDataProviderCache dataProviderCache,
            ILogger<ObservationManager> logger)
        {
            _processedObservationRepository = processedObservationRepository ??
                                              throw new ArgumentNullException(nameof(processedObservationRepository));
            _protectedLogRepository =
                protectedLogRepository ?? throw new ArgumentNullException(nameof(protectedLogRepository));
            _vocabularyValueResolver = vocabularyValueResolver ?? throw new ArgumentNullException(nameof(vocabularyValueResolver));
            _filterManager = filterManager ?? throw new ArgumentNullException(nameof(filterManager));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _taxonObservationCountCache = taxonObservationCountCache ?? throw new ArgumentNullException(nameof(taxonObservationCountCache));
            _taxonSumAggregationCache = taxonSumAggregationCache ?? throw new ArgumentNullException(nameof(taxonSumAggregationCache));
            _generalizationResolver = generalizationResolver ?? throw new ArgumentNullException(nameof(generalizationResolver));
            _dataProviderCache = dataProviderCache ?? throw new ArgumentNullException(nameof(dataProviderCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Make sure we are working with live data
            _processedObservationRepository.LiveMode = true;
        }


        public int MaxNrElasticSearchAggregationBuckets => _processedObservationRepository.MaxNrElasticSearchAggregationBuckets;

        /// <inheritdoc />
        public async Task<PagedResult<dynamic>> GetChunkAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            int skip,
            int take)
        {
            try
            {
                await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);
                var processedObservations =
                    await _processedObservationRepository.GetChunkAsync(filter, skip, take);
                await PostProcessObservations(filter, filter.ExtendedAuthorization.ProtectionFilter, processedObservations.Records, filter.FieldTranslationCultureCode);

                return processedObservations;
            }
            catch (AuthenticationRequiredException)
            {
                throw;
            }
            catch (TimeoutException e)
            {
                _logger.LogError(e, "Get chunk of observations timeout");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get chunk of observations");
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<ScrollResult<dynamic>> GetObservationsByScrollAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            int take,
            string scrollId)
        {
            try
            {
                await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);
                var processedObservations =
                    await _processedObservationRepository.GetObservationsByScrollAsync(filter, take, scrollId);
                await PostProcessObservations(filter, filter.ExtendedAuthorization.ProtectionFilter, processedObservations.Records, filter.FieldTranslationCultureCode);
                return processedObservations;
            }
            catch (AuthenticationRequiredException)
            {
                throw;
            }
            catch (TimeoutException e)
            {
                _logger.LogError(e, "Get observations by scroll timeout");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get observations by scroll");
                return null;
            }
        }


        /// <inheritdoc />
        public async Task<PagedResult<dynamic>> GetAggregatedChunkAsync(
            int? roleId,
            string authorizationApplicationIdentifier, SearchFilter filter, AggregationType aggregationType, int skip, int take)
        {
            try
            {
                await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);

                if (aggregationType.IsDateHistogram())
                    return await _processedObservationRepository.GetAggregatedHistogramChunkAsync(filter, aggregationType);

                if (aggregationType.IsSpeciesSightingsList())
                    return await _processedObservationRepository.GetAggregatedChunkAsync(filter, aggregationType, skip, take);

                if (aggregationType == AggregationType.SightingsPerWeek48)
                    return await _processedObservationRepository.GetAggregated48WeekHistogramAsync(filter);

                return null;
            }
            catch (AuthenticationRequiredException)
            {
                throw;
            }
            catch (TimeoutException e)
            {
                _logger.LogError(e, "Get aggregated chunk of observations timeout");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get aggregated chunk of observations");
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<GeoGridTileResult> GetGeogridTileAggregationAsync(int? roleId, string authorizationApplicationIdentifier, SearchFilter filter, int precision)
        {
            try
            {
                await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);
                return await _processedObservationRepository.GetGeogridTileAggregationAsync(filter, precision);
            }
            catch (ArgumentOutOfRangeException e)
            {
                _logger.LogError(e, "Failed to aggregate to metric tiles. To many buckets");
                throw;
            }
            catch (TimeoutException e)
            {
                _logger.LogError(e, "Get geogrid tile aggregation timeout");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to aggregate to geogrids.");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<GeoGridMetricResult> GetMetricGridAggregationAsync(int? roleId, string authorizationApplicationIdentifier,
                SearchFilter filter, int gridCellSizeInMeters, MetricCoordinateSys metricCoordinateSys)
        {
            try
            {
                await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);
                var result = await _processedObservationRepository.GetMetricGridAggregationAsync(filter, gridCellSizeInMeters, metricCoordinateSys);

                return result;
            }
            catch (ArgumentOutOfRangeException e)
            {
                _logger.LogError(e, "Failed to aggregate to metric tiles. To many buckets");
                throw;
            }
            catch (TimeoutException e)
            {
                _logger.LogError(e, "Aggregate to metric tiles timeout");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to aggregate to metric tiles.");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<DateTime?> GetLatestModifiedDateForProviderAsync(int providerId)
        {
            return await _processedObservationRepository.GetLatestModifiedDateForProviderAsync(providerId);
        }

        /// <inheritdoc />
        public async Task<long> GetMatchCountAsync(int? roleId, string authorizationApplicationIdentifier, SearchFilterBase filter, bool skipAuthorizationFilters = false)
        {
            await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);
            return await _processedObservationRepository.GetMatchCountAsync(filter, skipAuthorizationFilters);
        }

        public async Task<IEnumerable<TaxonObservationCountDto>> GetCachedCountAsync(SearchFilterBase filter, TaxonObservationCountSearch taxonObservationCountSearch)
        {
            var taxonObservationCountCacheKey = TaxonObservationCountCacheKey.Create(taxonObservationCountSearch);
            var countByTaxonId = new Dictionary<int, TaxonCount>();
            var notCachedTaxonIds = new HashSet<int>();
            foreach (var taxonId in taxonObservationCountSearch.TaxonIds)
            {
                taxonObservationCountCacheKey.TaxonId = taxonId;
                if (_taxonObservationCountCache.TryGetCount(taxonObservationCountCacheKey, out var count))
                {
                    countByTaxonId.Add(taxonId, count);
                }
                else
                {
                    notCachedTaxonIds.Add(taxonId);
                }
            }

            if (notCachedTaxonIds.Any())
            {
                foreach (var notCachedTaxonId in notCachedTaxonIds)
                {
                    filter.Taxa.Ids = new[] { notCachedTaxonId };
                    int observationCount = Convert.ToInt32(await GetMatchCountAsync(null, null, filter));
                    int provinceCount = Convert.ToInt32(await GetProvinceCountAsync(null, null, filter));
                    var taxonCount = new TaxonCount { ObservationCount = observationCount, ProvinceCount = provinceCount };
                    var cacheKey = TaxonObservationCountCacheKey.Create(taxonObservationCountSearch, notCachedTaxonId);
                    countByTaxonId.Add(notCachedTaxonId, taxonCount);
                    _taxonObservationCountCache.Add(cacheKey, taxonCount);
                }
            }

            var taxonCountDtos = countByTaxonId
                .Select(m => new TaxonObservationCountDto
                {
                    TaxonId = m.Key,
                    ObservationCount = m.Value.ObservationCount,
                    ProvinceCount = m.Value.ProvinceCount
                });

            return taxonCountDtos;
        }

        /// <inheritdoc />
        public async Task<SignalSerachResult> SignalSearchInternalAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            int areaBuffer,
            bool onlyAboveMyClearance,
            bool validateGeographic)
        {
            try
            {
                await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter, "SightingIndication", areaBuffer, filter?.Location?.Geometries?.UsePointAccuracy, filter?.Location?.Geometries?.UseDisturbanceRadius);

                if (!filter.ExtendedAuthorization?.ExtendedAreas?.Any() ?? true)
                {
                    throw new AuthenticationRequiredException("User don't have the SightingIndication permission that is required");
                }
                if (validateGeographic && filter.Location?.AreaGeographic != null)
                {
                    var areaGeographic = filter.Location?.AreaGeographic;
                    var hasAccess = true;
                    if ((areaGeographic.CountryRegionIds?.Count() ?? 0) != 0)
                    {
                        hasAccess = hasAccess && filter.ExtendedAuthorization.ExtendedAreas.Exists(ea => (ea.GeographicAreas?.CountryRegionIds?.Intersect(areaGeographic.CountryRegionIds)?.Count() ?? 0) > 0);
                    }
                    if ((areaGeographic.CountyIds?.Count() ?? 0) != 0)
                    {
                        hasAccess = hasAccess && filter.ExtendedAuthorization.ExtendedAreas.Exists(ea => (ea.GeographicAreas?.CountyIds?.Intersect(areaGeographic.CountyIds)?.Count() ?? 0) > 0);
                    }
                    if ((areaGeographic.MunicipalityIds?.Count() ?? 0) != 0)
                    {
                        hasAccess = hasAccess && filter.ExtendedAuthorization.ExtendedAreas.Exists(ea => (ea.GeographicAreas?.MunicipalityIds?.Intersect(areaGeographic.MunicipalityIds)?.Count() ?? 0) > 0);
                    }
                    if ((areaGeographic.ParishIds?.Count() ?? 0) != 0)
                    {
                        hasAccess = hasAccess && filter.ExtendedAuthorization.ExtendedAreas.Exists(ea => (ea.GeographicAreas?.ParishIds?.Intersect(areaGeographic.ParishIds)?.Count() ?? 0) > 0);
                    }
                    if ((areaGeographic.ProvinceIds?.Count() ?? 0) != 0)
                    {
                        hasAccess = hasAccess && filter.ExtendedAuthorization.ExtendedAreas.Exists(ea => (ea.GeographicAreas?.ProvinceIds?.Intersect(areaGeographic.ProvinceIds)?.Count() ?? 0) > 0);
                    }
                    if (!hasAccess)
                    {
                        return SignalSerachResult.NoPermissions;
                    }

                    if (areaGeographic.GeometryFilter?.Geometries?.Any() ?? false)
                    {
                        // Check if user has access to provided geometries
                        foreach (var geoShape in areaGeographic.GeometryFilter.Geometries)
                        {
                            var geometry = geoShape.ToGeometry();
                            if (!filter.ExtendedAuthorization.ExtendedAreas.Exists(ea => ea.GeographicAreas?.GeometryFilter?.Geometries?.Exists(g => g.ToGeometry().Contains(geometry)) ?? false))
                            {
                                return SignalSerachResult.NoPermissions;
                            }
                        }
                    }
                    if (filter.Location.Geometries.BoundingBox != null)
                    {
                        var bbGeometry = filter.Location.Geometries.BoundingBox.ToGeoemtry();
                        if (!filter.ExtendedAuthorization.ExtendedAreas.Exists(ea => ea.GeographicAreas?.GeometryFilter?.Geometries?.Exists(g => g.ToGeometry().Contains(bbGeometry)) ?? false))
                        {
                            return SignalSerachResult.NoPermissions;
                        }
                    }
                }
              
                var result = await _processedObservationRepository.SignalSearchInternalAsync(filter, onlyAboveMyClearance);
                return result ? SignalSerachResult.Yes : SignalSerachResult.No;
            }
            catch (TimeoutException e)
            {
                _logger.LogError(e, "Aggregate to metric tiles timeout");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Signal search failed");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<string>> TryToGetOccurenceIdDuplicatesAsync(bool protectedIndex, int maxReturnedItems)
        {
            return await _processedObservationRepository.TryToGetOccurenceIdDuplicatesAsync(protectedIndex, maxReturnedItems);
        }

        /// <inheritdoc />
        public async Task<dynamic> GetObservationAsync(int? userId, int? roleId, string authorizationApplicationIdentifier, string occurrenceId, OutputFieldSet outputFieldSet,
            string translationCultureCode, bool protectedObservations, bool includeInternalFields, bool resolveGeneralizedObservations)
        {
            var protectionFilter = protectedObservations ? ProtectionFilter.Sensitive : ProtectionFilter.Public;
            if (!protectedObservations && resolveGeneralizedObservations)
            {
                protectionFilter = ProtectionFilter.BothPublicAndSensitive;
            }
            var filter = includeInternalFields ?
                new SearchFilterInternal(userId ?? 0, protectionFilter) { NotPresentFilter = SightingNotPresentFilter.IncludeNotPresent } :
                new SearchFilter(userId ?? 0, protectionFilter);
            filter.Output = new OutputFilter();
            filter.Output.PopulateFields(outputFieldSet);

            await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter, "Sighting", null, null, null, false);

            var processedObservation = await _processedObservationRepository.GetObservationAsync(occurrenceId, filter);

            await PostProcessObservations(filter, protectionFilter, processedObservation, translationCultureCode);

            return (processedObservation?.Count ?? 0) == 1 ? processedObservation[0] : null;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<YearCountResultDto>> GetUserYearCountAsync(SearchFilter filter)
        {
            try
            {
                // Make sure mandatory properties is set
                filter.ExtendedAuthorization.ObservedByMe = true;
                filter.ExtendedAuthorization.ProtectionFilter = ProtectionFilter.Public; // Since we have set ObservedByMe, we don't need authorization check (always acces to own observations)
                filter.DiffusionStatuses = new List<DiffusionStatus> { DiffusionStatus.NotDiffused };
                await _filterManager.PrepareFilterAsync(null, null, filter);

                if (filter.ExtendedAuthorization.UserId == 0)
                {
                    throw new AuthenticationRequiredException("You have to login in order to use this end point");
                }

                var result = await _processedObservationRepository.GetUserYearCountAsync(filter);
                return result?.ToDtos();
            }
            catch (TimeoutException e)
            {
                _logger.LogError(e, "Get user year count timeout");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get user year count");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<YearMonthCountResultDto>> GetUserYearMonthCountAsync(SearchFilter filter)
        {
            try
            {
                // Make sure mandatory properties is set
                filter.ExtendedAuthorization.ObservedByMe = true;
                filter.ExtendedAuthorization.ProtectionFilter = ProtectionFilter.Public; // Since we have set ObservedByMe, we don't need authorization check (always acces to own observations)
                filter.DiffusionStatuses = new List<DiffusionStatus> { DiffusionStatus.NotDiffused };
                await _filterManager.PrepareFilterAsync(null, null, filter);

                if (filter.ExtendedAuthorization.UserId == 0)
                {
                    throw new AuthenticationRequiredException("You have to login in order to use this end point");
                }

                var result = await _processedObservationRepository.GetUserYearMonthCountAsync(filter);
                return result?.ToDtos();
            }
            catch (TimeoutException e)
            {
                _logger.LogError(e, "Get user year, month count timeout");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get user year, month count");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<YearMonthDayCountResultDto>> GetUserYearMonthDayCountAsync(SearchFilter filter, int skip, int take)
        {
            try
            {
                // Make sure mandatory properties is set
                filter.ExtendedAuthorization.ObservedByMe = true;
                filter.ExtendedAuthorization.ProtectionFilter = ProtectionFilter.Public; // Since we have set ObservedByMe, we don't need authorization check (always acces to own observations)
                filter.DiffusionStatuses = new List<DiffusionStatus> { DiffusionStatus.NotDiffused };
                await _filterManager.PrepareFilterAsync(null, null, filter);

                if (filter.ExtendedAuthorization.UserId == 0)
                {
                    throw new AuthenticationRequiredException("You have to login in order to use this end point");
                }

                var result = await _processedObservationRepository.GetUserYearMonthDayCountAsync(filter, skip, take);
                return result?.ToDtos();
            }
            catch (TimeoutException e)
            {
                _logger.LogError(e, "Get user year, month count timeout");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get user year, month count");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> HasIndexOccurrenceIdDuplicatesAsync(bool protectedIndex)
        {
            return await _processedObservationRepository.HasIndexOccurrenceIdDuplicatesAsync(protectedIndex);
        }

        /// <inheritdoc />
        public async Task<long> IndexCountAsync(bool protectedIndex = false)
        {
            return await _processedObservationRepository.IndexCountAsync(protectedIndex);
        }

        public async Task<Dictionary<string, ObservationStatistics>> CalculateObservationStatisticsAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var result = new Dictionary<string, ObservationStatistics>();
                var dataProviders = await _dataProviderCache.GetAllAsync();
                TimeSpan delay = TimeSpan.FromMilliseconds(100); // delay between calls to Elasticsearch.
                ObservationStatistics statisticsBefore = null;

                var searchFilter = new SearchFilter()
                {
                    DataProviderIds = dataProviders.Select(m => m.Id).ToList(),
                    Date = new DateFilter
                    {
                        DateFilterType = DateFilter.DateRangeFilterType.OverlappingStartDateAndEndDate
                    },
                    Taxa = new TaxonFilter
                    {

                    }
                };

                toDate = new DateTime(toDate.Year, toDate.Month, DateTime.DaysInMonth(toDate.Year, toDate.Month), 23, 59, 59);
                DateTime current = new DateTime(fromDate.Year, fromDate.Month, 1);
                current -= TimeSpan.FromDays(1);
                current = new DateTime(current.Year, current.Month, DateTime.DaysInMonth(current.Year, current.Month), 23, 59, 59);

                // Loop through each month
                while (current <= toDate)
                {
                    searchFilter.Date.EndDate = current;
                    string dateKey = $"Until {current:yyyy-MM-dd}";
                    result.Add(dateKey, new ObservationStatistics());

                    // Total
                    searchFilter = new SearchFilter()
                    {
                        DataProviderIds = dataProviders.Select(m => m.Id).ToList(),
                        Date = new DateFilter
                        {
                            DateFilterType = DateFilter.DateRangeFilterType.OverlappingStartDateAndEndDate,
                            EndDate = current
                        },
                        VerificationStatus = SearchFilterBase.StatusVerification.BothVerifiedAndNotVerified,
                        ExtendedAuthorization = new ExtendedAuthorizationFilter
                        {
                            ProtectionFilter = ProtectionFilter.BothPublicAndSensitive
                        }
                    };
                    result[dateKey].TotalCount = (int)await GetMatchCountAsync(null, null, searchFilter, true);                    

                    // Verified
                    searchFilter = new SearchFilter()
                    {
                        DataProviderIds = dataProviders.Select(m => m.Id).ToList(),
                        Date = new DateFilter
                        {
                            DateFilterType = DateFilter.DateRangeFilterType.OverlappingStartDateAndEndDate,
                            EndDate = current
                        },
                        VerificationStatus = SearchFilterBase.StatusVerification.Verified,
                        ExtendedAuthorization = new ExtendedAuthorizationFilter
                        {
                            ProtectionFilter = ProtectionFilter.BothPublicAndSensitive
                        }
                    };
                    result[dateKey].VerifiedCount = (int)await GetMatchCountAsync(null, null, searchFilter, true);

                    // Redlisted
                    searchFilter = new SearchFilter()
                    {
                        DataProviderIds = dataProviders.Select(m => m.Id).ToList(),
                        Date = new DateFilter
                        {
                            DateFilterType = DateFilter.DateRangeFilterType.OverlappingStartDateAndEndDate,
                            EndDate = current
                        },
                        Taxa = new TaxonFilter
                        {
                            RedListCategories = ["CR", "EN", "VU", "NT"]
                        },
                        VerificationStatus = SearchFilterBase.StatusVerification.BothVerifiedAndNotVerified,
                        ExtendedAuthorization = new ExtendedAuthorizationFilter
                        {
                            ProtectionFilter = ProtectionFilter.BothPublicAndSensitive
                        }
                    };
                    result[dateKey].RedlistedCount = (int)await GetMatchCountAsync(null, null, searchFilter, true);

                    // Invasive
                    searchFilter = new SearchFilter()
                    {
                        DataProviderIds = dataProviders.Select(m => m.Id).ToList(),
                        Date = new DateFilter
                        {
                            DateFilterType = DateFilter.DateRangeFilterType.OverlappingStartDateAndEndDate,
                            EndDate = current
                        },
                        Taxa = new TaxonFilter
                        {
                            ListIds = [3]
                        },
                        VerificationStatus = SearchFilterBase.StatusVerification.BothVerifiedAndNotVerified,
                        ExtendedAuthorization = new ExtendedAuthorizationFilter
                        {
                            ProtectionFilter = ProtectionFilter.BothPublicAndSensitive
                        }
                    };
                    result[dateKey].InvasiveCount = (int)await GetMatchCountAsync(null, null, searchFilter, true);


                    // Calculate by data provider
                    foreach (var dataprovider in dataProviders)
                    {
                        // Total
                        searchFilter = new SearchFilter()
                        {
                            DataProviderIds = [dataprovider.Id],
                            Date = new DateFilter
                            {
                                DateFilterType = DateFilter.DateRangeFilterType.OverlappingStartDateAndEndDate,
                                EndDate = current
                            },
                            VerificationStatus = SearchFilterBase.StatusVerification.BothVerifiedAndNotVerified,
                            ExtendedAuthorization = new ExtendedAuthorizationFilter
                            {
                                ProtectionFilter = ProtectionFilter.BothPublicAndSensitive
                            }
                        };
                        await Task.Delay(delay);
                        var totalCountForProvider = await GetMatchCountAsync(null, null, searchFilter, true);

                        // Verified
                        searchFilter = new SearchFilter()
                        {
                            DataProviderIds = [dataprovider.Id],
                            Date = new DateFilter
                            {
                                DateFilterType = DateFilter.DateRangeFilterType.OverlappingStartDateAndEndDate,
                                EndDate = current
                            },
                            VerificationStatus = SearchFilterBase.StatusVerification.Verified,
                            ExtendedAuthorization = new ExtendedAuthorizationFilter
                            {
                                ProtectionFilter = ProtectionFilter.BothPublicAndSensitive
                            }
                        };
                        await Task.Delay(delay);
                        var verifiedCountForProvider = await GetMatchCountAsync(null, null, searchFilter, true);

                        // Redlisted
                        searchFilter = new SearchFilter()
                        {
                            DataProviderIds = [dataprovider.Id],
                            Date = new DateFilter
                            {
                                DateFilterType = DateFilter.DateRangeFilterType.OverlappingStartDateAndEndDate,
                                EndDate = current
                            },
                            Taxa = new TaxonFilter
                            {
                                RedListCategories = ["CR", "EN", "VU", "NT"]
                            },
                            VerificationStatus = SearchFilterBase.StatusVerification.BothVerifiedAndNotVerified,
                            ExtendedAuthorization = new ExtendedAuthorizationFilter
                            {
                                ProtectionFilter = ProtectionFilter.BothPublicAndSensitive
                            }
                        };
                        await Task.Delay(delay);
                        var redlistedCountForProvider = await GetMatchCountAsync(null, null, searchFilter, true);

                        // Invasive
                        searchFilter = new SearchFilter()
                        {
                            DataProviderIds = [dataprovider.Id],
                            Date = new DateFilter
                            {
                                DateFilterType = DateFilter.DateRangeFilterType.OverlappingStartDateAndEndDate,
                                EndDate = current
                            },
                            Taxa = new TaxonFilter
                            {
                                ListIds = [3]
                            },
                            VerificationStatus = SearchFilterBase.StatusVerification.BothVerifiedAndNotVerified,
                            ExtendedAuthorization = new ExtendedAuthorizationFilter
                            {
                                ProtectionFilter = ProtectionFilter.BothPublicAndSensitive
                            }
                        };
                        await Task.Delay(delay);
                        var invasiveCountForProvider = await GetMatchCountAsync(null, null, searchFilter, true);

                        result[dateKey].ObservationCountByProvider.Add(dataprovider.Names.First().Value, new ObservationCountTuple()
                        {
                            TotalCount = (int)totalCountForProvider,
                            RedlistedCount = (int)redlistedCountForProvider,
                            VerifiedCount = (int)verifiedCountForProvider,
                            InvasiveCount = (int)invasiveCountForProvider
                        });
                        await Task.Delay(delay);
                    }
                   
                    if (statisticsBefore != null)
                    {
                        var diffStatistics = CreateDiff(statisticsBefore, result[dateKey]);
                        string betweenDateKey = $"{current:yyyy-MM-01} to {current:yyyy-MM-dd}";
                        result.Add(betweenDateKey, diffStatistics);
                    }
                    
                    statisticsBefore = result[dateKey];

                    // Move to the last day of the next month
                    current = current.AddDays(1);
                    current = new DateTime(current.Year, current.Month, DateTime.DaysInMonth(current.Year, current.Month), 23, 59, 59);
                }
                
                result.Remove(result.First().Key);
                return result;
            }
            catch (TimeoutException e)
            {
                _logger.LogError(e, "Get NV statistics timeout");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get NV statistics");
                throw;
            }
        }

        private ObservationStatistics CreateDiff(ObservationStatistics before, ObservationStatistics after)
        {
            ObservationStatistics result = new ObservationStatistics();
            result.TotalCount = after.TotalCount - before.TotalCount;
            result.RedlistedCount = after.RedlistedCount - before.RedlistedCount;
            result.VerifiedCount = after.VerifiedCount - before.VerifiedCount;
            result.InvasiveCount = after.InvasiveCount - before.InvasiveCount;
            result.ObservationCountByProvider = CreateDiff(before.ObservationCountByProvider, after.ObservationCountByProvider);

            return result;
        }

        private Dictionary<string, int> CreateDiff(Dictionary<string, int> before, Dictionary<string, int> after)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            foreach (var pair in after)
            {
                if (before.TryGetValue(pair.Key, out int beforeValue))
                {
                    result.Add(pair.Key, pair.Value - beforeValue);
                }
                else
                {
                    result.Add(pair.Key, pair.Value);
                }
            }

            return result;
        }

        private Dictionary<string, ObservationCountTuple> CreateDiff(Dictionary<string, ObservationCountTuple> before, Dictionary<string, ObservationCountTuple> after)
        {
            Dictionary<string, ObservationCountTuple> result = new Dictionary<string, ObservationCountTuple>();
            foreach (var pair in after)
            {
                if (before.TryGetValue(pair.Key, out ObservationCountTuple beforeValue))
                {
                    result.Add(pair.Key, new ObservationCountTuple
                    {
                        TotalCount = pair.Value.TotalCount - beforeValue.TotalCount,
                        VerifiedCount = pair.Value.VerifiedCount - beforeValue.VerifiedCount,
                        RedlistedCount = pair.Value.RedlistedCount - beforeValue.RedlistedCount,
                        InvasiveCount = pair.Value.InvasiveCount - beforeValue.InvasiveCount
                    });
                }
                else
                {
                    result.Add(pair.Key, pair.Value);
                }
            }

            return result;
        }

        public async Task<byte[]> CreateObservationStatisticsSummaryExcelFileAsync(DateTime fromDate, DateTime toDate)
        {
            var statisticsByDate = await CalculateObservationStatisticsAsync(fromDate, toDate);
            var summaryExcelWriter = new ObservationStatisticsSummaryExcelWriter();
            byte[] excelFile = await summaryExcelWriter.CreateExcelFileAsync(statisticsByDate);
            return excelFile;
        }

        public class ObservationStatistics
        {
            public int TotalCount { get; set; }
            public int RedlistedCount { get; set; }
            public int VerifiedCount { get; set; }
            public int InvasiveCount { get; set; }
            public Dictionary<string, int> TotalCountByProvider { get; set; }
            public Dictionary<string, int> RedlistedCountByProvider { get; set; }
            public Dictionary<string, int> VerifiedCountByProvider { get; set; }
            public Dictionary<string, int> InvasiveCountByProvider { get; set; }
            public Dictionary<string, ObservationCountTuple> ObservationCountByProvider { get; set; } = new Dictionary<string, ObservationCountTuple>();
        }

        public class ObservationCountTuple
        {
            public int TotalCount { get; set; }
            public int RedlistedCount { get; set; }
            public int VerifiedCount { get; set; }
            public int InvasiveCount { get; set; }
        }

        public class ObservationStatisticsSummaryExcelWriter
        {
            public async Task<byte[]> CreateExcelFileAsync(Dictionary<string, ObservationStatistics> statisticsByDate)
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var package = new ExcelPackage();
                foreach (var item in statisticsByDate)
                {
                    CreateWorksheet(package, item.Key, item.Value);
                }

                byte[] excelBytes = await package.GetAsByteArrayAsync();
                return excelBytes;
            }

            private ExcelWorksheet CreateWorksheet(ExcelPackage package, string title, ObservationStatistics observationStatistics)
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(title);
                worksheet.Cells[1, 1].Value = title;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.UnderLine = true;

                worksheet.Cells[2, 1].Value = "# Total observations";
                worksheet.Cells[2, 1].Style.Font.Bold = true;
                worksheet.Cells[2, 2].Value = observationStatistics.TotalCount;
                worksheet.Cells[2, 2].Style.Numberformat.Format = "#,##0";

                worksheet.Cells[3, 1].Value = "# Verified observations";
                worksheet.Cells[3, 1].Style.Font.Bold = true;
                worksheet.Cells[3, 2].Value = observationStatistics.VerifiedCount;
                worksheet.Cells[3, 2].Style.Numberformat.Format = "#,##0";

                worksheet.Cells[4, 1].Value = "# Redlisted observations";
                worksheet.Cells[4, 1].Style.Font.Bold = true;
                worksheet.Cells[4, 2].Value = observationStatistics.RedlistedCount;
                worksheet.Cells[4, 2].Style.Numberformat.Format = "#,##0";

                worksheet.Cells[5, 1].Value = "# Invasive observations";
                worksheet.Cells[5, 1].Style.Font.Bold = true;
                worksheet.Cells[5, 2].Value = observationStatistics.InvasiveCount;
                worksheet.Cells[5, 2].Style.Numberformat.Format = "#,##0";

                worksheet.Cells[7, 1].Value = "Dataprovider";
                worksheet.Cells[7, 1].Style.Font.Bold = true;
                worksheet.Cells[7, 2].Value = "# Total";
                worksheet.Cells[7, 2].Style.Font.Bold = true;
                worksheet.Cells[7, 3].Value = "# Verified";
                worksheet.Cells[7, 3].Style.Font.Bold = true;
                worksheet.Cells[7, 4].Value = "# Redlisted";
                worksheet.Cells[7, 4].Style.Font.Bold = true;
                worksheet.Cells[7, 5].Value = "# Invasive";
                worksheet.Cells[7, 5].Style.Font.Bold = true;
                int row = 8;
                foreach (var pair in observationStatistics.ObservationCountByProvider)
                {
                    worksheet.Cells[row, 1].Value = pair.Key;
                    worksheet.Cells[row, 2].Value = pair.Value.TotalCount;
                    worksheet.Cells[row, 2].Style.Numberformat.Format = "#,##0";
                    worksheet.Cells[row, 3].Value = pair.Value.VerifiedCount;
                    worksheet.Cells[row, 3].Style.Numberformat.Format = "#,##0";
                    worksheet.Cells[row, 4].Value = pair.Value.RedlistedCount;
                    worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0";
                    worksheet.Cells[row, 5].Value = pair.Value.InvasiveCount;
                    worksheet.Cells[row, 5].Style.Numberformat.Format = "#,##0";

                    row++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                return worksheet;
            }
        }
    }
}