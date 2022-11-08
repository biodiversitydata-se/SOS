using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Elasticsearch.Net;
using Nest;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.DataQuality;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Models.Shared;

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
        /// <returns></returns>
        Task<int> AddManyAsync(IEnumerable<Observation> observations, bool protectedIndex);

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
        /// <returns></returns>
        Task<bool> DeleteAllDocumentsAsync(bool protectedIndex);

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
        ///  Get aggregation in metric tiles 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="gridCellSizeInMeters"></param>
        /// <param name="metricCoordinateSys"></param>
        /// <returns></returns>
        Task<GeoGridMetricResult> GetMetricGridAggregationAsync(
            SearchFilter filter, 
            int gridCellSizeInMeters,
            MetricCoordinateSys metricCoordinateSys);

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
        /// <returns></returns>
        Task<long> GetMatchCountAsync(SearchFilterBase filter);

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
        /// Scroll measurement or facts
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="scrollId"></param>
        /// <returns></returns>
        Task<ScrollResult<ExtendedMeasurementOrFactRow>> ScrollMeasurementOrFactsAsync(
            SearchFilterBase filter,
            string scrollId = null);

        /// <summary>
        /// Scroll multimedia
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="scrollId"></param>
        /// <returns></returns>
        Task<ScrollResult<SimpleMultimediaRow>> ScrollMultimediaAsync(
            SearchFilterBase filter,
            string scrollId = null);

        /// <summary>
        /// Scroll observations
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="scrollId"></param>
        /// <returns></returns>
        Task<ScrollResult<T>> ScrollObservationsAsync<T>(
            SearchFilterBase filter,
            string scrollId);

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
        
        Task<IEnumerable<AggregationItem>> GetAggregationItemsAsync(SearchFilter filter, string aggregationField);
        Task<IEnumerable<AggregationItem>> GetAggregationItemsAsync(SearchFilter filter,
            string aggregationField,
            string numericSortField,
            int skip,
            int take);

        Task<List<AggregationItem>> GetAllAggregationItemsAsync(SearchFilter filter, string aggregationField);
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
    }
}