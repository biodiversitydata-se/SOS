﻿using Elasticsearch.Net;
using Nest;
using SOS.Lib.Enums;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.DataQuality;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Enums;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Models.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Lib.Repositories.Processed.Interfaces
{
    /// <summary>
    /// </summary>
    public interface IProcessedObservationCoreRepository : IProcessRepositoryBase<Observation, string>
    {
        /// <summary>
        ///  Add many items
        /// </summary>
        /// <param name="observations"></param>
        /// <param name="protectedIndex"></param>
        /// <param name="refreshIndex"></param>
        /// <returns></returns>
        int AddMany(IEnumerable<Observation> observations, bool protectedIndex, bool refreshIndex = false);

        /// <summary>
        ///  Add many items
        /// </summary>
        /// <param name="observations"></param>
        /// <param name="protectedIndex"></param>
        /// <param name="refreshIndex"></param>
        /// <returns></returns>
        Task<int> AddManyAsync(IEnumerable<Observation> observations, bool protectedIndex, bool refreshIndex = false);

        /// <summary>
        /// Clear the collection
        /// </summary>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        Task<bool> ClearCollectionAsync(bool protectedIndex);

        /// <summary>
        /// Delete collection
        /// </summary>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        Task<bool> DeleteCollectionAsync(bool protectedIndex);

        /// <summary>
        /// Delete all documents.
        /// </summary>
        /// <param name="protectedIndex"></param>
        /// <param name="waitForCompletion"></param>
        /// <returns></returns>
        Task<bool> DeleteAllDocumentsAsync(bool protectedIndex, bool waitForCompletion = false);

        /// <summary>
        ///  Delete observations by occurence id
        /// </summary>
        /// <param name="occurenceIds"></param>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        Task<bool> DeleteByOccurrenceIdAsync(IEnumerable<string> occurenceIds, bool protectedIndex);

        /// <summary>
        ///  Delete provider data.
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        Task<bool> DeleteProviderDataAsync(DataProvider dataProvider, bool protectedIndex);

        /// <summary>
        /// Turn of indexing
        /// </summary>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        Task<bool> DisableIndexingAsync(bool protectedIndex);

        /// <summary>
        /// Turn on indexing
        /// </summary>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        Task EnableIndexingAsync(bool protectedIndex);

        /// <summary>
        /// Ensure there are no duplictes in the index
        /// </summary>
        /// <returns></returns>
        Task<bool> EnsureNoDuplicatesAsync();

        /// <summary>
        /// Get a data quality report for passed organism group
        /// </summary>
        /// <param name="organismGroup"></param>
        /// <returns></returns>
        Task<DataQualityReport> GetDataQualityReportAsync(string organismGroup);

        /// <summary>
        /// Get current disk usage
        /// </summary>
        /// <returns></returns>
        int GetDiskUsage();

        /// <summary>
        /// Get aggregation in metric tiles 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="gridCellSizeInMeters"></param>
        /// <param name="metricCoordinateSys"></param>
        /// <param name="skipAuthorizationFilters"></param>
        /// <param name="maxBuckets"></param>
        /// <param name="afterKey"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        Task<GeoGridMetricResult> GetMetricGridAggregationAsync(
            SearchFilter filter,
            int gridCellSizeInMeters,
            MetricCoordinateSys metricCoordinateSys,
            bool skipAuthorizationFilters = false,
            int? maxBuckets = null,
            CompositeKey afterKey = null,
            TimeSpan? timeout = null);

        /// <summary>
        /// Get index health status
        /// </summary>
        /// <param name="waitForStatus"></param>
        /// <param name="waitForSeconds"></param>
        /// <returns></returns>
        Task<WaitForStatus> GetHealthStatusAsync(WaitForStatus waitForStatus, int waitForSeconds);

        /// <summary>
        /// Get latest data modified date for passed provider 
        /// </summary>
        /// <param name="providerId"></param>
        /// <returns></returns>
        Task<DateTime> GetLatestModifiedDateForProviderAsync(int providerId);

        /// <summary>
        /// Get number of matches for query
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skipAuthorizationFilters"></param>
        /// <returns></returns>
        Task<long> GetMatchCountAsync(SearchFilterBase filter, bool skipAuthorizationFilters = false);

        /// <summary>
        /// Get observations by their occurrence id's
        /// </summary>
        /// <param name="occurrenceIds"></param>
        /// <param name="outputFields"></param>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        Task<IEnumerable<Observation>> GetObservationsAsync(
            IEnumerable<string> occurrenceIds,
            IEnumerable<string> outputFields,
            bool protectedIndex);

        /// <summary>
        /// Get observations by occurrence id's
        /// </summary>
        /// <param name="occurrenceIds"></param>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        Task<IEnumerable<Observation>> GetObservationsAsync(IEnumerable<string> occurrenceIds, bool protectedIndex);

        /// <summary>
        /// Get all project id's matching filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<IEnumerable<int>> GetProjectIdsAsync(SearchFilter filter);

        /// <summary>
        /// Get measurement or facts by using search after
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="pointInTimeId"></param>
        /// <param name="searchAfter"></param>
        /// <returns></returns>
        Task<SearchAfterResult<ExtendedMeasurementOrFactRow>> GetMeasurementOrFactsBySearchAfterAsync(
            SearchFilterBase filter,
            string pointInTimeId = null,
            IEnumerable<object> searchAfter = null);

        /// <summary>
        /// Get multimedia  by using search after
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="pointInTimeId"></param>
        /// <param name="searchAfter"></param>
        /// <returns></returns>
        Task<SearchAfterResult<SimpleMultimediaRow>> GetMultimediaBySearchAfterAsync(
            SearchFilterBase filter,
            string pointInTimeId = null,
            IEnumerable<object> searchAfter = null);

        /// <summary>
        /// Get observations by using search after
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="pointInTimeId"></param>
        /// <param name="searchAfter"></param>
        /// <returns></returns>
        Task<SearchAfterResult<T>> GetObservationsBySearchAfterAsync<T>(
            SearchFilter filter,
            string pointInTimeId = null,
            IEnumerable<object> searchAfter = null);

        Task<ScrollResult<dynamic>> GetObservationsByScrollAsync(
            SearchFilter filter,
            int take,
            string scrollId);

        /// <summary>
        /// Get provider meta data
        /// </summary>
        /// <param name="providerId"></param>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        Task<(DateTime? firstSpotted, DateTime? lastSpotted, GeoBounds geographicCoverage)> GetProviderMetaDataAsync(
            int providerId, bool protectedIndex);

        /// <summary>
        /// Get random observations
        /// </summary>
        /// <param name="take"></param>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        Task<IEnumerable<Observation>> GetRandomObservationsAsync(int take, bool protectedIndex);

        /// <summary>
        /// Returns url to first host
        /// </summary>
        Uri HostUrl { get; }

        /// <summary>
        /// Count documents in index
        /// </summary>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        Task<long> IndexCountAsync(bool protectedIndex);

        /// <summary>
        /// Name of public index 
        /// </summary>
        string PublicIndexName { get; }

        /// <summary>
        /// Name of protected index 
        /// </summary>
        string ProtectedIndexName { get; }

        /// <summary>
        /// Look for duplicates
        /// </summary>
        /// <param name="protectedIndex"></param>
        /// <param name="maxReturnedItems"></param>
        /// <returns></returns>
        Task<IEnumerable<string>> TryToGetOccurenceIdDuplicatesAsync(bool protectedIndex, int maxReturnedItems);

        /// <summary>
        /// Unique name of public index 
        /// </summary>
        string UniquePublicIndexName { get; }

        /// <summary>
        /// Unique name of protected index 
        /// </summary>
        string UniqueProtectedIndexName { get; }

        /// <summary>
        ///  Make sure no protected observations are in public index or no public observations are in protected index
        /// </summary>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        Task<bool> ValidateProtectionLevelAsync(bool protectedIndex);

        /// <summary>
        /// Verify that collection exists
        /// </summary>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        Task<bool> VerifyCollectionAsync(bool protectedIndex);

        Task<IEnumerable<AggregationItem>> GetAggregationItemsAsync(SearchFilter filter,
            string aggregationField,
            int? precisionThreshold = null,
            int size = 65536,
            AggregationSortOrder sortOrder = AggregationSortOrder.CountDescending);

        Task<PagedResult<AggregationItem>> GetAggregationItemsAsync(SearchFilter filter,
            string aggregationField,
            int skip,
            int take,
            int? precisionThreshold,
            AggregationSortOrder sortOrder = AggregationSortOrder.CountDescending);

        Task<IEnumerable<AggregationItemOrganismQuantity>> GetAggregationItemsAggregateOrganismQuantityAsync(SearchFilter filter,
            string aggregationField,
            int? precisionThreshold,
            int size = 65536,
            AggregationSortOrder sortOrder = AggregationSortOrder.CountDescending);

        /// <summary>
        /// Aggregate by a field and return the number of documents for each value.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="aggregationField"></param>
        /// <returns></returns>
        Task<List<AggregationItem>> GetAllAggregationItemsAsync(SearchFilter filter, string aggregationField);

        /// <summary>
        /// Aggregate by a key field and a list field and return the distinct value list for each key field.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="filter"></param>
        /// <param name="aggregationFieldKey"></param>
        /// <param name="aggregationFieldList"></param>
        /// <returns></returns>
        Task<List<AggregationItemList<TKey, TValue>>> GetAllAggregationItemsListAsync<TKey, TValue>(SearchFilter filter, string aggregationFieldKey, string aggregationFieldList);

        /// <summary>
        /// Get occurrence ids for events.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<List<EventOccurrenceAggregationItem>> GetEventOccurrenceItemsAsync(SearchFilter filter);

        /// <summary>
        ///     Get chunk of objects from repository
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="getAllFields">If true all observation fields will be retrieved.</param>
        /// <returns></returns>
        Task<PagedResult<dynamic>> GetChunkAsync(SearchFilter filter, int skip, int take, bool getAllFields = false);

        /// <summary>
        /// Gets a single observation
        /// </summary>
        /// <param name="occurrenceId"></param>
        /// <param name="filter"></param>
        /// <param name="getAllFields">If true all observation fields will be retrieved.</param>
        /// <returns></returns>
        Task<dynamic> GetObservationAsync(string occurrenceId, SearchFilter filter, bool getAllFields = false);

        /// <summary>
        /// Get a list of sortable fields
        /// </summary>
        /// <returns></returns>
        Task<HashSet<string>> GetSortableFieldsAsync();

        /// <summary>
        /// Wait for public index to be created.
        /// </summary>
        /// <param name="expectedRecordsCount"></param>
        /// <param name="timeout"></param>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        Task WaitForPublicIndexCreationAsync(long expectedRecordsCount, TimeSpan? timeout = null, bool protectedIndex = false);

        /// <summary>
        /// Aggregate by user passed field
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="aggregationField"></param>
        /// <param name="aggregateOrganismQuantity"></param>
        /// <param name="precisionThreshold"></param>
        /// <param name="afterKey"></param>
        /// <param name="take"></param>
        /// <param name="useScript">If aggregation is made on mandatory field, you don't need to aggregate on script field. 
        /// Setting this to false will make a faster aggregation, but empty value and null will be handled different</param>
        /// <returns></returns>
        Task<SearchAfterResult<dynamic>> AggregateByUserFieldAsync(SearchFilter filter, string aggregationField, bool aggregateOrganismQuantity, int? precisionThreshold, string? afterKey = null, int? take = 10, bool? useScript = true);
    }
}