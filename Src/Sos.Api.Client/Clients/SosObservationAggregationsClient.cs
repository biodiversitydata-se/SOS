using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Requests;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Aggregations.Responses;
using SOS.Api.Contracts.Common.Responses;
using GeoJSON.Text.Feature;

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
    public Task<IReadOnlyDictionary<int, TaxonAreaAggregationDto>> TaxonAreaAggregationInternalAsync(
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
    public Task<IReadOnlyDictionary<int, TaxonAreaAggregationDto>> TaxonAreaAggregationInternalJsonAsync(
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
        using var request = CreatePostRequest(
            BuildTimeSeriesHistogramInternalUri(requestOptions),
            filterJson,
            requestOptions.RoleId,
            requestOptions.AuthorizationApplicationIdentifier);

        _requestLogging.LogRequest(_httpClient, request, filterJson);
        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        var results = await SosClientHttp.ReadJsonResponseAsync<List<TimeSeriesHistogramResultDto>>(
            response,
            request.RequestUri,
            cancellationToken);

        return results ?? [];
    }

    private async Task<GeoGridResultDto?> SendGeoGridAggregationInternalAsync(
        string filterJson,
        GeoGridAggregationInternalOptions? options,
        CancellationToken cancellationToken)
    {
        var requestOptions = options ?? new GeoGridAggregationInternalOptions();
        using var request = CreatePostRequest(
            BuildGeoGridAggregationInternalUri(requestOptions),
            filterJson,
            requestOptions.RoleId,
            requestOptions.AuthorizationApplicationIdentifier);

        _requestLogging.LogRequest(_httpClient, request, filterJson);
        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        return await SosClientHttp.ReadJsonResponseAsync<GeoGridResultDto>(
            response,
            request.RequestUri,
            cancellationToken);
    }

    private async Task<GeoGridMetricResultDto?> SendMetricGridAggregationInternalAsync(
        string filterJson,
        MetricGridAggregationInternalOptions? options,
        CancellationToken cancellationToken)
    {
        var requestOptions = options ?? new MetricGridAggregationInternalOptions();
        using var request = CreatePostRequest(
            BuildMetricGridAggregationInternalUri(requestOptions),
            filterJson,
            requestOptions.RoleId,
            requestOptions.AuthorizationApplicationIdentifier);

        _requestLogging.LogRequest(_httpClient, request, filterJson);
        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        return await SosClientHttp.ReadJsonResponseAsync<GeoGridMetricResultDto>(
            response,
            request.RequestUri,
            cancellationToken);
    }

    private async Task<FeatureCollection> SendMetricGridAggregationInternalGeoJsonAsync(
        string filterJson,
        MetricGridAggregationInternalGeoJsonOptions? options,
        CancellationToken cancellationToken)
    {
        var requestOptions = options ?? new MetricGridAggregationInternalGeoJsonOptions();
        using var request = CreatePostRequest(
            BuildMetricGridAggregationInternalUri(requestOptions),
            filterJson,
            requestOptions.RoleId,
            requestOptions.AuthorizationApplicationIdentifier,
            SosAcceptProfile.GeoJson);

        _requestLogging.LogRequest(_httpClient, request, filterJson);
        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        return await SosClientHttp.ReadJsonResponseAsync<FeatureCollection>(
                   response,
                   request.RequestUri,
                   cancellationToken)
               ?? new FeatureCollection();
    }

    private async Task<PagedResultDto<TaxonAggregationItemDto>?> SendTaxonAggregationInternalAsync(
        string filterJson,
        TaxonAggregationInternalOptions? options,
        CancellationToken cancellationToken)
    {
        var requestOptions = options ?? new TaxonAggregationInternalOptions();
        using var request = CreatePostRequest(
            BuildTaxonAggregationInternalUri(requestOptions),
            filterJson,
            requestOptions.RoleId,
            requestOptions.AuthorizationApplicationIdentifier);

        _requestLogging.LogRequest(_httpClient, request, filterJson);
        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        return await SosClientHttp.ReadJsonResponseAsync<PagedResultDto<TaxonAggregationItemDto>>(
            response,
            request.RequestUri,
            cancellationToken);
    }

    private async Task<IReadOnlyDictionary<int, TaxonAreaAggregationDto>> SendTaxonAreaAggregationInternalAsync(
        string filterJson,
        TaxonAreaAggregationInternalOptions? options,
        CancellationToken cancellationToken)
    {
        var requestOptions = options ?? new TaxonAreaAggregationInternalOptions();
        using var request = CreatePostRequest(
            BuildTaxonAreaAggregationInternalUri(requestOptions),
            filterJson,
            requestOptions.RoleId,
            requestOptions.AuthorizationApplicationIdentifier);

        _requestLogging.LogRequest(_httpClient, request, filterJson);
        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        var result = await SosClientHttp.ReadJsonResponseAsync<Dictionary<int, TaxonAreaAggregationDto>>(
            response,
            request.RequestUri,
            cancellationToken);

        return result ?? throw SosClientHttp.CreateInvalidResponseException(response, request.RequestUri);
    }

    private static HttpRequestMessage CreatePostRequest(
        Uri uri,
        string filterJson,
        int? roleId,
        string? authorizationApplicationIdentifier,
        SosAcceptProfile acceptProfile = SosAcceptProfile.Json)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = SosClientHttp.CreateJsonContent(filterJson)
        };
        request.ApplySosRequestHeaders(
            roleId,
            authorizationApplicationIdentifier,
            acceptProfile);
        return request;
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
