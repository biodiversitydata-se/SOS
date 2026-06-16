using NetTopologySuite.Geometries;
using SOS.Lib.Dtos.Filter;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search.Filters;

namespace SOS.Lib.Helpers.Interfaces;

public interface IMarkdownHelper
{
    /// <summary>
    /// Generates a Markdown-formatted citation for the specified source string.
    /// </summary>
    /// <param name="authorizationApplicationIdentifier">The source string to include in the citation.</param>
    /// <param name="dataProviderIds">A collection of data provider IDs to include in the citation.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A memory stream containing the generated Markdown content.</returns>
    Task<MemoryStream> GetCitationMarkDownAsync(string authorizationApplicationIdentifier, IEnumerable<int> dataProviderIds, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously generates a Markdown-formatted export of datasets that match the specified search filter.
    /// </summary>
    /// <param name="searchFilter">The search criteria used to filter datasets for export. Can be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a memory stream with the generated
    /// Markdown content and a collection of data provider IDs.</returns>
    Task<(MemoryStream FileStream, IEnumerable<int> DataProviderIds)> GetDatasetsMarkDownAsync(SearchFilter? searchFilter, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously generates a Markdown-formatted export of species observations that match the specified search
    /// filter.
    /// </summary>
    /// <remarks>The caller is responsible for disposing the returned memory stream. The method may return an
    /// empty stream if no observations match the filter.</remarks>
    /// <param name="filter">The search criteria used to filter species observations for export. Cannot be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a memory stream with the generated
    /// Markdown content.</returns>
    Task<MemoryStream> GetFilterMarkDownAsync(SearchFilterBaseDto filter, CancellationToken cancellationToken);

    Task<Stream> GetAnalysisMarkDownAsync(
        SearchFilterAggregationInternalDto? searchFilter,
        double[]? alphaValues,
        int? gridCellSizeInMeters,
        bool? useCenterPoint,
        bool? useEdgeLengthRatio,
        bool? allowHoles,
        bool? returnGridCells,
        bool? includeEmptyCells,
        MetricCoordinateSys? metricCoordinateSys,
        CoordinateSys? coordinateSystem,
        CancellationToken cancellationToken);

    Task<Stream> GetAnalysisMarkDownAsync(
        SearchFilterBaseDto? searchFilter,
        int? maxDistance,
        int? gridCellSizeInMeters,
        MetricCoordinateSys? metricCoordinateSys,
        CoordinateSys? coordinateSystem,
        CancellationToken cancellationToken);

    /// <summary>
    /// Attempts to create a memory stream containing a feature collection representation of the specified geometries.
    /// </summary>
    /// <remarks>The returned stream is positioned at the beginning and should be disposed by the caller when
    /// no longer needed.</remarks>
    /// <param name="geometries">An enumerable collection of geometry objects to include in the feature collection. Cannot be null.</param>
    /// <returns>A memory stream containing the serialized feature collection if successful; otherwise, null.</returns>
    MemoryStream TryGetFeatureCollection(IEnumerable<Geometry> geometries);
}
