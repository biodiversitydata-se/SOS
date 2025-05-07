using Hangfire;
using NetTopologySuite.Features;
using SOS.Lib.Enums;
using SOS.Lib.Models.Analysis;
using SOS.Lib.Models.Export;
using SOS.Lib.Models.Search.Enums;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Lib.Managers.Interfaces
{
    public interface IAnalysisManager
    {
        /// <summary>
        /// Aggregate by user passed field
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="aggregationField"></param>
        /// <param name="aggregateOrganismQuantity"></param>
        /// <param name="precisionThreshold"></param>
        /// <param name="afterKey"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<PagedAggregationResult<UserAggregationResponse>> AggregateByUserFieldAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            string aggregationField,
            bool aggregateOrganismQuantity,
            int? precisionThreshold,
            string afterKey,
            int? take);

        /// <summary>
        /// Aggregate by user passed field
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="aggregationField"></param>
        /// <param name="aggregateOrganismQuantity"></param>
        /// <param name="precisionThreshold"></param>
        /// <param name="take"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        Task<IEnumerable<AggregationItemOrganismQuantity>> AggregateByUserFieldAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            string aggregationField,
            bool aggregateOrganismQuantity,
            int? precisionThreshold,
            int take,
            AggregationSortOrder sortOrder = AggregationSortOrder.CountDescending);

        /// <summary>
        /// Aggreagate on area
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="areaType"></param>
        /// <param name="precisionThreshold"></param>
        /// <param name="aggregateOrganismQuantity"></param>
        /// <param name="coordinateSys"></param>
        /// <returns></returns>
        Task<FeatureCollection> AreaAggregateAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            AreaTypeAggregate areaType,
            int? precisionThreshold,
            bool? aggregateOrganismQuantity,
            CoordinateSys coordinateSys);

        /// <summary>
        /// Calculate AOO and EOO
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="gridCellsInMeters"></param>
        /// <param name="useCenterPoint"></param>
        /// <param name="alphaValues"></param>
        /// <param name="useEdgeLengthRatio"></param>
        /// <param name="allowHoles"></param>
        /// <param name="returnGridCells"></param>
        /// <param name="includeEmptyCells"></param>
        /// <param name="metricCoordinateSys"></param>
        /// <param name="coordinateSystem"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        Task<(FeatureCollection FeatureCollection, List<AooEooItem> AooEooItems)?> CalculateAooAndEooAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            int gridCellsInMeters,
            bool useCenterPoint,
            IEnumerable<double> alphaValues,
            bool useEdgeLengthRatio,
            bool allowHoles,
            bool returnGridCells,
            bool includeEmptyCells,
            MetricCoordinateSys metricCoordinateSys,
            CoordinateSys coordinateSystem,
            TimeSpan? timeout = null);

        /// <summary>
        /// Calculate AOO EOO for Article 17
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="gridCellsInMeters"></param>
        /// <param name="maxDistance"></param>
        /// <param name="metricCoordinateSys"></param>
        /// <param name="coordinateSystem"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        Task<(FeatureCollection FeatureCollection, AooEooItem AooEooItem)?> CalculateAooAndEooArticle17Async(
           int? roleId,
           string authorizationApplicationIdentifier,
           SearchFilter filter,
           int gridCellsInMeters,
           int maxDistance,
           MetricCoordinateSys metricCoordinateSys,
           CoordinateSys coordinateSystem,
           TimeSpan? timeout = null);

        /// <summary>
        /// Get count of observations matching search criteria
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<long> GetMatchCountAsync(int? roleId, string authorizationApplicationIdentifier, SearchFilterBase filter);

        Task<int> GetNumberOfTaxaInFilterAsync(SearchFilterBase filter);

        Task<FileExportResult> CreateAooEooExportAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            int gridCellsInMeters,
            bool useCenterPoint,
            IEnumerable<double> alphaValues,
            bool useEdgeLengthRatio,
            bool allowHoles,
            bool returnGridCells,
            bool includeEmptyCells,
            MetricCoordinateSys metricCoordinateSys,
            CoordinateSys coordinateSystem,
            string exportPath,
            string fileName,            
            IJobCancellationToken cancellationToken);

        Task<FileExportResult> CreateAooAndEooArticle17ExportAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            int gridCellsInMeters,
            int maxDistance,
            MetricCoordinateSys metricCoordinateSys,
            CoordinateSys coordinateSystem,
            string exportPath,
            string fileName,
            IJobCancellationToken cancellationToken);
    }
}