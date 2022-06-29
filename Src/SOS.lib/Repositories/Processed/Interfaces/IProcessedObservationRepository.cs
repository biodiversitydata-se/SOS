using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Http;
using Nest;
using SOS.Lib.Enums;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.DataQuality;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.Repositories.Processed.Interfaces
{
    /// <summary>
    /// </summary>
    public interface IProcessedObservationRepository : IProcessRepositoryBase<Observation, string>
    {
        /// <summary>
        /// Http context accessor.
        /// </summary>
        IHttpContextAccessor HttpContextAccessor { get; set; }

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
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="aggregationType"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<PagedResult<dynamic>> GetAggregatedChunkAsync(SearchFilter filter, AggregationType aggregationType, int skip, int take);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="aggregationType"></param>
        /// <returns></returns>
        Task<PagedResult<dynamic>> GetAggregatedHistogramChunkAsync(SearchFilter filter, AggregationType aggregationType);

        /// <summary>
        ///     Get chunk of objects from repository
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="sortBy"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        Task<PagedResult<dynamic>> GetChunkAsync(SearchFilter filter, int skip, int take, string sortBy,
            SearchSortOrder sortOrder);

        /// <summary>
        /// Get a data quality report for passed organism group
        /// </summary>
        /// <param name="organismGroup"></param>
        /// <returns></returns>
        Task<DataQualityReport> GetDataQualityReportAsync(string organismGroup);

        Task<Result<GeoGridTileResult>> GetGeogridTileAggregationAsync(SearchFilter filter, int zoom);

        /// <summary>
        /// Get aggregation in metric tiles 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="gridCellSizeInMeters"></param>
        /// <returns></returns>
        Task<Result<GeoGridMetricResult>> GetMetricGridAggregationAsync(
            SearchFilter filter, int gridCellSizeInMeters);
        /// <summary>
        /// Get index health status
        /// </summary>
        /// <param name="waitForStatus"></param>
        /// <returns></returns>
        Task<WaitForStatus> GetHealthStatusAsync(WaitForStatus waitForStatus);

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
        /// Get number of provinces matching the provided filter.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<int> GetProvinceCountAsync(SearchFilterBase filter);

        /// <summary>
        /// Gets a single observation
        /// </summary>
        /// <param name="occurrenceId"></param>
        /// <returns></returns>
        Task<dynamic> GetObservationAsync(string occurrenceId, SearchFilter filter);

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
            string sortBy,
            SearchSortOrder sortOrder,
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
        /// Count the number of user observations group by year
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<IEnumerable<YearCountResult>> GetUserYearCountAsync(SearchFilter filter);

        /// <summary>
        /// Count the number of user observations group by year and month
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<IEnumerable<YearMonthCountResult>> GetUserYearMonthCountAsync(SearchFilter filter);

        /// <summary>
        /// Count the number of user observations group by year, month and day
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IEnumerable<YearMonthDayCountResult>> GetUserYearMonthDayCountAsync(SearchFilter filter, int skip, int take);

        /// <summary>
        /// Check if index have observations with same occurrence id
        /// </summary>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        Task<bool> HasIndexOccurrenceIdDuplicatesAsync(bool protectedIndex);

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
        /// Signal search
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="onlyAboveMyClearance"></param>
        /// <returns></returns>
        Task<bool> SignalSearchInternalAsync(
            SearchFilter filter,
            bool onlyAboveMyClearance);

        /// <summary>
        /// Look for duplicates
        /// </summary>
        /// <param name="activeInstance"></param>
        /// <param name="protectedIndex"></param>
        /// <param name="maxReturnedItems"></param>
        /// <returns></returns>
        Task<IEnumerable<string>> TryToGetOccurenceIdDuplicatesAsync(bool activeInstance, bool protectedIndex, int maxReturnedItems);

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
    }
}