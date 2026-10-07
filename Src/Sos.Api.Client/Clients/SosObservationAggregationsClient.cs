using GeoJSON.Text.Feature;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Requests;
using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Aggregations.Responses;
using SOS.Api.Contracts.Common.Responses;
using SOS.Api.Contracts.Observations.Requests;

namespace SOS.Api.Client.Clients;

/// <inheritdoc />
public sealed class SosObservationAggregationsClient : ISosObservationAggregationsClient
{
    private readonly HttpClient _httpClient;
    private readonly SosRequestLoggingContext _requestLogging;

    /// <summary>Creates an observation-aggregation client using the supplied HTTP client.</summary>
    public SosObservationAggregationsClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _requestLogging = SosRequestLoggingContext.Disabled;
    }

    /// <summary>Creates an observation-aggregation client with opt-in request logging.</summary>
    [Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructor]
    public SosObservationAggregationsClient(
        HttpClient httpClient,
        ILogger<SosObservationAggregationsClient> logger,
        IOptions<SosClientOptions> options)
        : this(httpClient)
    {
        _requestLogging = new SosRequestLoggingContext(logger, options);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TaxonSumAggregationItem>?> CachedCountInternalAsync(
        IEnumerable<int> taxonIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(taxonIds);
        return await SendAsync<IEnumerable<TaxonSumAggregationItem>>(
            new Uri("observations/internal/cachedcount", UriKind.Relative),
            SosClientHttp.Serialize(taxonIds),
            new SosRoleRequestOptions(),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<GeoGridResultDto?> GeoGridAggregationInternalAsync(
        SearchFilterAggregationInternalDto filter,
        GeoGridAggregationInternalOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return SendGeoGridAggregationInternalAsync(
            SosClientHttp.Serialize(filter),
            options,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<GeoGridResultDto?> GeoGridAggregationInternalJsonAsync(
        string filterJson,
        GeoGridAggregationInternalOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        SosClientHttp.ThrowIfEmptyJson(filterJson, nameof(filterJson));
        return SendGeoGridAggregationInternalAsync(filterJson, options, cancellationToken);
    }

    /// <inheritdoc />
    public Task<GeoGridMetricResultDto?> MetricGridAggregationInternalAsync(
        SearchFilterAggregationInternalDto filter,
        MetricGridAggregationInternalOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return SendMetricGridAggregationInternalAsync(
            SosClientHttp.Serialize(filter),
            options,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<GeoGridMetricResultDto?> MetricGridAggregationInternalJsonAsync(
        string filterJson,
        MetricGridAggregationInternalOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        SosClientHttp.ThrowIfEmptyJson(filterJson, nameof(filterJson));
        return SendMetricGridAggregationInternalAsync(filterJson, options, cancellationToken);
    }

    /// <inheritdoc />
    public Task<FeatureCollection> MetricGridAggregationInternalGeoJsonAsync(
        SearchFilterAggregationInternalDto filter,
        MetricGridAggregationInternalGeoJsonOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return SendMetricGridAggregationInternalGeoJsonAsync(
            SosClientHttp.Serialize(filter),
            options,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<FeatureCollection> MetricGridAggregationInternalGeoJsonAsync(
        string filterJson,
        MetricGridAggregationInternalGeoJsonOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        SosClientHttp.ThrowIfEmptyJson(filterJson, nameof(filterJson));
        return SendMetricGridAggregationInternalGeoJsonAsync(filterJson, options, cancellationToken);
    }

    /// <inheritdoc />
    public Task<PagedResultDto<TaxonAggregationItemDto>?> TaxonAggregationInternalAsync(
        SearchFilterAggregationInternalDto filter,
        TaxonAggregationInternalOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return SendTaxonAggregationInternalAsync(
            SosClientHttp.Serialize(filter),
            options,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<PagedResultDto<TaxonAggregationItemDto>?> TaxonAggregationInternalJsonAsync(
        string filterJson,
        TaxonAggregationInternalOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        SosClientHttp.ThrowIfEmptyJson(filterJson, nameof(filterJson));
        return SendTaxonAggregationInternalAsync(filterJson, options, cancellationToken);
    }

    /// <inheritdoc />
    public Task<PagedResultDto<TaxonSumAggregationItem>?> TaxonSumAggregationInternalAsync(
        TaxonFilterDto filter,
        TaxonSumAggregationInternalOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return SendTaxonSumAggregationInternalAsync(
            SosClientHttp.Serialize(filter),
            options,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<PagedResultDto<TaxonSumAggregationItem>?> TaxonSumAggregationInternalJsonAsync(
        string filterJson,
        TaxonSumAggregationInternalOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        SosClientHttp.ThrowIfEmptyJson(filterJson, nameof(filterJson));
        return SendTaxonSumAggregationInternalAsync(filterJson, options, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyDictionary<int, TaxonAreaAggregation>> TaxonAreaAggregationInternalAsync(
        SearchFilterAggregationInternalDto filter,
        TaxonAreaAggregationInternalOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return SendTaxonAreaAggregationInternalAsync(
            SosClientHttp.Serialize(filter),
            options,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyDictionary<int, TaxonAreaAggregation>> TaxonAreaAggregationInternalJsonAsync(
        string filterJson,
        TaxonAreaAggregationInternalOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        SosClientHttp.ThrowIfEmptyJson(filterJson, nameof(filterJson));
        return SendTaxonAreaAggregationInternalAsync(filterJson, options, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TimeSeriesHistogramResultDto>> TimeSeriesHistogramInternalAsync(
        SearchFilterAggregationInternalDto filter,
        TimeSeriesHistogramInternalOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return await TimeSeriesHistogramInternalJsonAsync(
            SosClientHttp.Serialize(filter),
            options,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TimeSeriesHistogramResultDto>> TimeSeriesHistogramInternalJsonAsync(
        string filterJson,
        TimeSeriesHistogramInternalOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        SosClientHttp.ThrowIfEmptyJson(filterJson, nameof(filterJson));
        var requestOptions = options ?? new TimeSeriesHistogramInternalOptions();
        var results = await SendAsync<List<TimeSeriesHistogramResultDto>>(
            BuildTimeSeriesHistogramInternalUri(requestOptions),
            filterJson,
            requestOptions,
            cancellationToken);

        return results ?? [];
    }

    private Task<GeoGridResultDto?> SendGeoGridAggregationInternalAsync(
        string filterJson,
        GeoGridAggregationInternalOptions? options,
        CancellationToken cancellationToken)
    {
        var requestOptions = options ?? new GeoGridAggregationInternalOptions();
        return SendAsync<GeoGridResultDto>(
            BuildGeoGridAggregationInternalUri(requestOptions),
            filterJson,
            requestOptions,
            cancellationToken);
    }

    private Task<GeoGridMetricResultDto?> SendMetricGridAggregationInternalAsync(
        string filterJson,
        MetricGridAggregationInternalOptions? options,
        CancellationToken cancellationToken)
    {
        var requestOptions = options ?? new MetricGridAggregationInternalOptions();
        return SendAsync<GeoGridMetricResultDto>(
            BuildMetricGridAggregationInternalUri(requestOptions),
            filterJson,
            requestOptions,
            cancellationToken);
    }

    private async Task<FeatureCollection> SendMetricGridAggregationInternalGeoJsonAsync(
        string filterJson,
        MetricGridAggregationInternalGeoJsonOptions? options,
        CancellationToken cancellationToken)
    {
        var requestOptions = options ?? new MetricGridAggregationInternalGeoJsonOptions();
        return await SendAsync<FeatureCollection>(
                   BuildMetricGridAggregationInternalUri(requestOptions),
                   filterJson,
                   requestOptions,
                   cancellationToken,
                   SosAcceptProfile.GeoJson)
               ?? new FeatureCollection();
    }

    private Task<PagedResultDto<TaxonAggregationItemDto>?> SendTaxonAggregationInternalAsync(
        string filterJson,
        TaxonAggregationInternalOptions? options,
        CancellationToken cancellationToken)
    {
        var requestOptions = options ?? new TaxonAggregationInternalOptions();
        return SendAsync<PagedResultDto<TaxonAggregationItemDto>>(
            BuildTaxonAggregationInternalUri(requestOptions),
            filterJson,
            requestOptions,
            cancellationToken);
    }

    private Task<PagedResultDto<TaxonSumAggregationItem>?> SendTaxonSumAggregationInternalAsync(
        string filterJson,
        TaxonSumAggregationInternalOptions? options,
        CancellationToken cancellationToken)
    {
        var requestOptions = options ?? new TaxonSumAggregationInternalOptions();
        return SendAsync<PagedResultDto<TaxonSumAggregationItem>>(
            BuildTaxonSumAggregationInternalUri(requestOptions),
            filterJson,
            new SosRoleRequestOptions(),
            cancellationToken);
    }

    private async Task<IReadOnlyDictionary<int, TaxonAreaAggregation>> SendTaxonAreaAggregationInternalAsync(
        string filterJson,
        TaxonAreaAggregationInternalOptions? options,
        CancellationToken cancellationToken)
    {
        var requestOptions = options ?? new TaxonAreaAggregationInternalOptions();
        return (await SendAsync<Dictionary<int, TaxonAreaAggregation>>(
            BuildTaxonAreaAggregationInternalUri(requestOptions),
            filterJson,
            requestOptions,
            cancellationToken,
            requireResponseBody: true))!;
    }

    private async Task<T?> SendAsync<T>(
        Uri uri,
        string filterJson,
        SosRoleRequestOptions options,
        CancellationToken cancellationToken,
        SosAcceptProfile acceptProfile = SosAcceptProfile.Json,
        bool requireResponseBody = false)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = SosClientHttp.CreateJsonContent(filterJson)
        };
        request.ApplySosRequestHeaders(
            options.RoleId,
            options.AuthorizationApplicationIdentifier,
            acceptProfile);

        _requestLogging.LogRequest(_httpClient, request, filterJson);
        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        var result = await SosClientHttp.ReadJsonResponseAsync<T>(
            response,
            request.RequestUri,
            cancellationToken);
        if (result is null && requireResponseBody)
        {
            throw SosClientHttp.CreateInvalidResponseException(response, request.RequestUri);
        }
        return result;
    }

    private static Uri BuildGeoGridAggregationInternalUri(GeoGridAggregationInternalOptions options)
    {
        return new Uri(
            $"observations/internal/geogridaggregation?{options.ToQueryString()}",
            UriKind.Relative);
    }

    private static Uri BuildMetricGridAggregationInternalUri(MetricGridAggregationInternalOptions options)
    {
        return new Uri(
            $"observations/internal/metricgridaggregation?{options.ToQueryString()}",
            UriKind.Relative);
    }

    private static Uri BuildMetricGridAggregationInternalUri(MetricGridAggregationInternalGeoJsonOptions options)
    {
        return new Uri(
            $"observations/internal/metricgridaggregation?{options.ToQueryString()}",
            UriKind.Relative);
    }

    private static Uri BuildTaxonAggregationInternalUri(TaxonAggregationInternalOptions options)
    {
        return new Uri(
            $"observations/internal/taxonaggregation?{options.ToQueryString()}",
            UriKind.Relative);
    }

    private static Uri BuildTaxonSumAggregationInternalUri(TaxonSumAggregationInternalOptions options)
    {
        return new Uri(
            $"observations/internal/taxonsumaggregation?{options.ToQueryString()}",
            UriKind.Relative);
    }

    private static Uri BuildTaxonAreaAggregationInternalUri(TaxonAreaAggregationInternalOptions options)
    {
        return new Uri(
            $"observations/internal/taxonareaaggregation?{options.ToQueryString()}",
            UriKind.Relative);
    }

    private static Uri BuildTimeSeriesHistogramInternalUri(TimeSeriesHistogramInternalOptions options)
    {
        return new Uri(
            $"observations/internal/timeserieshistogram?{options.ToQueryString()}",
            UriKind.Relative);
    }
}
