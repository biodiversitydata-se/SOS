using SOS.Api.Client.Requests;
using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Aggregations.Responses;
using SOS.Api.Contracts.Common.Responses;
using GeoJSON.Text.Feature;

namespace SOS.Api.Client.Abstractions;

/// <summary>Client for SOS observation aggregation endpoints.</summary>
public interface ISosObservationAggregationsClient
{
    /// <summary>Aggregates observations into geographic grid cells.</summary>
    Task<GeoGridResultDto?> GeoGridAggregationInternalAsync(
        SearchFilterAggregationInternalDto filter,
        GeoGridAggregationInternalOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Aggregates a JSON-encoded observation filter into geographic grid cells.</summary>
    Task<GeoGridResultDto?> GeoGridAggregationInternalJsonAsync(
        string filterJson,
        GeoGridAggregationInternalOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Aggregates observations into metric grid cells.</summary>
    Task<GeoGridMetricResultDto?> MetricGridAggregationInternalAsync(
        SearchFilterAggregationInternalDto filter,
        MetricGridAggregationInternalOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Aggregates a JSON-encoded observation filter into metric grid cells.</summary>
    Task<GeoGridMetricResultDto?> MetricGridAggregationInternalJsonAsync(
        string filterJson,
        MetricGridAggregationInternalOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Aggregates observations into metric grid cells returned as GeoJSON.</summary>
    Task<FeatureCollection> MetricGridAggregationInternalGeoJsonAsync(
        SearchFilterAggregationInternalDto filter,
        MetricGridAggregationInternalGeoJsonOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Aggregates a JSON-encoded observation filter into metric grid cells returned as GeoJSON.</summary>
    Task<FeatureCollection> MetricGridAggregationInternalGeoJsonAsync(
        string filterJson,
        MetricGridAggregationInternalGeoJsonOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Aggregates observations by taxon.</summary>
    Task<PagedResultDto<TaxonAggregationItemDto>?> TaxonAggregationInternalAsync(
        SearchFilterAggregationInternalDto filter,
        TaxonAggregationInternalOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Aggregates a JSON-encoded observation filter by taxon.</summary>
    Task<PagedResultDto<TaxonAggregationItemDto>?> TaxonAggregationInternalJsonAsync(
        string filterJson,
        TaxonAggregationInternalOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Aggregates observations by taxon and area.</summary>
    Task<IReadOnlyDictionary<int, TaxonAreaAggregationDto>> TaxonAreaAggregationInternalAsync(
        SearchFilterAggregationInternalDto filter,
        TaxonAreaAggregationInternalOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Aggregates a JSON-encoded observation filter by taxon and area.</summary>
    Task<IReadOnlyDictionary<int, TaxonAreaAggregationDto>> TaxonAreaAggregationInternalJsonAsync(
        string filterJson,
        TaxonAreaAggregationInternalOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Aggregates observations into a time-series histogram.</summary>
    Task<IReadOnlyList<TimeSeriesHistogramResultDto>> TimeSeriesHistogramInternalAsync(
        SearchFilterAggregationInternalDto filter,
        TimeSeriesHistogramInternalOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Aggregates a JSON-encoded observation filter into a time-series histogram.</summary>
    Task<IReadOnlyList<TimeSeriesHistogramResultDto>> TimeSeriesHistogramInternalJsonAsync(
        string filterJson,
        TimeSeriesHistogramInternalOptions? options = null,
        CancellationToken cancellationToken = default);
}
