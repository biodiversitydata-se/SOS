using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Elasticsearch.Net;
using Nest;
using SOS.Lib.Enums;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.DataQuality;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.Repositories.Processed.Interfaces
{
    /// <summary>
    /// </summary>
    public interface IProcessedObservationRepository : IProcessRepositoryBase<Observation>
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
        /// Copy provider data from active instance to inactive instance.
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        Task<bool> CopyProviderDataAsync(DataProvider dataProvider, bool protectedIndex);

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

        Task<Result<IEnumerable<GeoGridTileTaxaCell>>> GetCompleteGeoTileTaxaAggregationAsync(
            SearchFilter filter,
            int zoom);

        /// <summary>
        /// Get a data quality report for passed organism group
        /// </summary>
        /// <param name="organismGroup"></param>
        /// <returns></returns>
        Task<DataQualityReport> GetDataQualityReportAsync(string organismGroup);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        Task<Result<GeoGridResult>> GetGeogridAggregationAsync(SearchFilter filter, int precision);

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
        /// Get locations by id
        /// </summary>
        /// <param name="locationIds"></param>
        /// <returns></returns>
        Task<IEnumerable<Location>> GetLocationsAsync(IEnumerable<string> locationIds);

        /// <summary>
        /// Get number of matches for query
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<long> GetMatchCountAsync(FilterBase filter);

        /// <summary>
        /// Get number of provinces matching the provided filter.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<int> GetProvinceCountAsync(FilterBase filter);

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

        Task<ScrollResult<dynamic>> GetObservationsByScrollAsync(
            SearchFilter filter,
            int take,
            string sortBy,
            SearchSortOrder sortOrder,
            string scrollId);

        
        Task<Result<GeoGridTileTaxonPageResult>> GetPageGeoTileTaxaAggregationAsync(
            SearchFilter filter,
            int zoom,
            string geoTilePage,
            int? taxonIdPage);

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
        /// Aggregate observations by taxon. Sort by observation count descending.
        /// </summary>
        /// <param name="filter"></param>        
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="sumUnderlyingTaxa"></param>
        /// <returns></returns>
        Task<Result<PagedResult<TaxonAggregationItem>>> GetTaxonAggregationAsync(
            SearchFilter filter,
            int? skip,
            int? take,
            bool sumUnderlyingTaxa = false);

        /// <summary>
        /// Get indication if taxa exists in specified area
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<IEnumerable<TaxonAggregationItem>> GetTaxonExistsIndicationAsync(
            SearchFilter filter);

        /// <summary>
        /// Count documents in index
        /// </summary>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        Task<long> IndexCountAsync(bool protectedIndex);

        /// <summary>
        /// Max number of aggregation buckets in ElasticSearch.
        /// </summary>
        int MaxNrElasticSearchAggregationBuckets { get; }

        /// <summary>
        /// Name of public index 
        /// </summary>
        string PublicIndexName { get; }

        /// <summary>
        /// Name of protected index 
        /// </summary>
        string ProtectedIndexName { get; }

        /// <summary>
        /// Get measurementOrFacts.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="scrollId"></param>
        /// <returns></returns>
        Task<ScrollResult<ExtendedMeasurementOrFactRow>> ScrollMeasurementOrFactsAsync(
            FilterBase filter,
            string scrollId);

        /// <summary>
        ///     Get multimedia.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="scrollId"></param>
        /// <returns></returns>
        Task<ScrollResult<SimpleMultimediaRow>> ScrollMultimediaAsync(
            FilterBase filter,
            string scrollId);

        /// <summary>
        ///     Get observation by scroll. 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="scrollId"></param>
        /// <returns></returns>
        Task<ScrollResult<Observation>> ScrollObservationsAsync(
            FilterBase filter,
            string scrollId);

        Task<ScrollResult<dynamic>> ScrollObservationsAsDynamicAsync(
            SearchFilter filter,
            string scrollId);

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

        /// <summary>
        /// Batch size used for write
        /// </summary>
        int WriteBatchSize { get; set; }
    }
}