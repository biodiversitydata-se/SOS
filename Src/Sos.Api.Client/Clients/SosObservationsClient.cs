using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Requests;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SOS.Api.Contracts.Common.Responses;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Observations.Responses;
using System.Net.Http.Headers;
using System.Text;

namespace SOS.Api.Client.Clients;

/// <inheritdoc />
public sealed class SosObservationsClient : ISosObservationsClient
{
    private readonly HttpClient _httpClient;
    private readonly SosRequestLoggingContext _requestLogging;

    /// <summary>Creates an observation client using the supplied HTTP client.</summary>
    public SosObservationsClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _requestLogging = SosRequestLoggingContext.Disabled;
    }

    /// <summary>Creates an observation client with opt-in request logging.</summary>
    [Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructor]
    public SosObservationsClient(
        HttpClient httpClient,
        ILogger<SosObservationsClient> logger,
        IOptions<SosClientOptions> options)
        : this(httpClient)
    {
        _requestLogging = new SosRequestLoggingContext(logger, options);
    }

    /// <inheritdoc />
    public Task<Observation?> GetObservationByIdAsync(
        string occurrenceId,
        ObservationByIdOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ThrowIfEmptyOccurrenceId(occurrenceId);
        var requestOptions = options ?? new ObservationByIdOptions();
        return SendGetObservationByIdAsync(
            $"observations/{Uri.EscapeDataString(occurrenceId)}?{requestOptions.ToQueryString()}",
            requestOptions.RoleId,
            requestOptions.AuthorizationApplicationIdentifier,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<Observation?> GetObservationByIdInternalAsync(
        string occurrenceId,
        ObservationByIdInternalOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ThrowIfEmptyOccurrenceId(occurrenceId);
        var requestOptions = options ?? new ObservationByIdInternalOptions();
        return SendGetObservationByIdAsync(
            $"observations/internal/{Uri.EscapeDataString(occurrenceId)}?{requestOptions.ToQueryString()}",
            requestOptions.RoleId,
            requestOptions.AuthorizationApplicationIdentifier,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<int> CountInternalAsync(
        SearchFilterInternalBaseDto filter,
        CountInternalOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return SendCountInternalAsync(
            SosClientHttp.Serialize(filter),
            options,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<int> CountInternalJsonAsync(
        string filterJson,
        CountInternalOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        SosClientHttp.ThrowIfEmptyJson(filterJson, nameof(filterJson));
        return SendCountInternalAsync(filterJson, options, cancellationToken);
    }

    /// <inheritdoc />
    public Task<AdaptiveSearchLocationsResponse> AdaptiveSearchLocationsInternalAsync(
        SearchFilterInternalDto filter,
        AdaptiveSearchLocationsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return SendAdaptiveSearchLocationsInternalAsync(
            SosClientHttp.Serialize(filter),
            options,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<AdaptiveSearchLocationsResponse> AdaptiveSearchLocationsInternalJsonAsync(
        string filterJson,
        AdaptiveSearchLocationsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filterJson))
        {
            throw new ArgumentException("Filter JSON must not be empty.", nameof(filterJson));
        }

        return SendAdaptiveSearchLocationsInternalAsync(filterJson, options, cancellationToken);
    }

    /// <inheritdoc />
    public Task<AdaptiveSearchResponse> AdaptiveSearchInternalAsync(
        SearchFilterInternalDto filter,
        AdaptiveSearchOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return SendAdaptiveSearchInternalAsync(
            SosClientHttp.Serialize(filter),
            options,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<AdaptiveSearchResponse> AdaptiveSearchInternalJsonAsync(
        string filterJson,
        AdaptiveSearchOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        SosClientHttp.ThrowIfEmptyJson(filterJson, nameof(filterJson));
        return SendAdaptiveSearchInternalAsync(filterJson, options, cancellationToken);
    }

    /// <inheritdoc />
    public Task<GeoPagedResultWithMetadataDto<Observation>> ObservationsBySearchInternalAsync(
        SearchFilterInternalDto filter,
        ObservationSearchOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return SendObservationsBySearchInternalAsync(
            SosClientHttp.Serialize(filter),
            options,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<GeoPagedResultWithMetadataDto<Observation>> ObservationsBySearchInternalAsync(
        string filterJson,
        ObservationSearchOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filterJson))
        {
            throw new ArgumentException("Filter JSON must not be empty.", nameof(filterJson));
        }

        return SendObservationsBySearchInternalAsync(filterJson, options, cancellationToken);
    }

    /// <inheritdoc />
    public Task<SearchByCursorResultDto<Observation>> ObservationsByCursorAsync(
        SearchFilterDto filter,
        SearchByCursorOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return SendObservationsByCursorAsync(
            SosClientHttp.Serialize(filter),
            options,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<SearchByCursorResultDto<Observation>> ObservationsByCursorJsonAsync(
        string filterJson,
        SearchByCursorOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filterJson))
        {
            throw new ArgumentException("Filter JSON must not be empty.", nameof(filterJson));
        }

        return SendObservationsByCursorAsync(filterJson, options, cancellationToken);
    }

    private async Task<int> SendCountInternalAsync(
        string filterJson,
        CountInternalOptions? options,
        CancellationToken cancellationToken)
    {
        var requestOptions = options ?? new CountInternalOptions();
        using var request = new HttpRequestMessage(HttpMethod.Post, BuildCountInternalUri(requestOptions))
        {
            Content = SosClientHttp.CreateJsonContent(filterJson)
        };

        request.ApplySosRequestHeaders(
            requestOptions.RoleId,
            requestOptions.AuthorizationApplicationIdentifier);

        _requestLogging.LogRequest(_httpClient, request, filterJson);
        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        return await SosClientHttp.ReadJsonResponseAsync<int>(
            response,
            request.RequestUri,
            cancellationToken);
    }

    private async Task<Observation?> SendGetObservationByIdAsync(
        string relativeUri,
        int? roleId,
        string? authorizationApplicationIdentifier,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(relativeUri, UriKind.Relative));
        request.ApplySosRequestHeaders(
            roleId,
            authorizationApplicationIdentifier);

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        return await SosClientHttp.ReadJsonResponseAsync<Observation>(
            response,
            request.RequestUri,
            cancellationToken);
    }

    private async Task<AdaptiveSearchResponse> SendAdaptiveSearchInternalAsync(
        string filterJson,
        AdaptiveSearchOptions? options,
        CancellationToken cancellationToken)
    {
        var requestOptions = options ?? new AdaptiveSearchOptions();
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            BuildAdaptiveSearchInternalUri(requestOptions))
        {
            Content = SosClientHttp.CreateJsonContent(filterJson)
        };

        var acceptProfile = requestOptions.OutputFormat == OutputFormatDto.Json
            ? SosAcceptProfile.Json
            : SosAcceptProfile.GeoJson;
        request.ApplySosRequestHeaders(
            requestOptions.RoleId,
            requestOptions.AuthorizationApplicationIdentifier,
            acceptProfile);

        _requestLogging.LogRequest(_httpClient, request, filterJson);
        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw SosClientHttp.CreateApiException(
                response,
                await SosClientHttp.ReadErrorResponsePreviewAsync(response, cancellationToken),
                request.RequestUri);
        }

        var responseBody = await SosClientHttp.ReadResponseBodyAsync(response, cancellationToken);
        return new AdaptiveSearchResponse
        {
            Content = responseBody,
            ContentType = response.Content.Headers.ContentType?.ToString(),
            ResponseSizeBytes = response.Content.Headers.ContentLength ?? Encoding.UTF8.GetByteCount(responseBody),
            ObservationsTotalCount = GetLongHeaderValue(response.Headers, "X-Observations-TotalCount"),
            ResultCount = GetLongHeaderValue(response.Headers, "X-Result-Count"),
            ResultType = GetHeaderValue(response.Headers, "X-Result-Type"),
            Zoom = GetIntHeaderValue(response.Headers, "X-Zoom"),
            ObservationsCount = GetLongHeaderValue(response.Headers, "X-Observations-Count")
        };
    }

    private async Task<AdaptiveSearchLocationsResponse> SendAdaptiveSearchLocationsInternalAsync(
        string filterJson,
        AdaptiveSearchLocationsOptions? options,
        CancellationToken cancellationToken)
    {
        var requestOptions = options ?? new AdaptiveSearchLocationsOptions();
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            BuildAdaptiveSearchLocationsInternalUri(requestOptions))
        {
            Content = SosClientHttp.CreateJsonContent(filterJson)
        };

        request.ApplySosRequestHeaders(
            requestOptions.RoleId,
            requestOptions.AuthorizationApplicationIdentifier,
            SosAcceptProfile.GeoJson);

        _requestLogging.LogRequest(_httpClient, request, filterJson);
        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw SosClientHttp.CreateApiException(
                response,
                await SosClientHttp.ReadErrorResponsePreviewAsync(response, cancellationToken),
                request.RequestUri);
        }

        var responseBody = await SosClientHttp.ReadResponseBodyAsync(response, cancellationToken);
        return new AdaptiveSearchLocationsResponse
        {
            GeoJson = responseBody,
            ResponseSizeBytes = response.Content.Headers.ContentLength ?? Encoding.UTF8.GetByteCount(responseBody),
            ObservationsTotalCount = GetLongHeaderValue(response.Headers, "X-Observations-TotalCount"),
            EstimatedLocationCount = GetLongHeaderValue(response.Headers, "X-Estimated-Location-Count"),
            FeatureCount = GetLongHeaderValue(response.Headers, "X-Feature-Count"),
            LocationsBeforeClustering = GetLongHeaderValue(response.Headers, "X-Locations-Before-Clustering"),
            Zoom = GetIntHeaderValue(response.Headers, "X-Zoom"),
            CacheStatus = GetHeaderValue(response.Headers, "X-Cache"),
            ResultType = GetHeaderValue(response.Headers, "X-Result-Type"),
            Timings = GetHeaderValue(response.Headers, "X-AdaptiveSearch-Timings"),
            LocationAggregationMode = GetHeaderValue(response.Headers, "X-Location-Aggregation-Mode"),
            LocationSummaryApproximated = GetBoolHeaderValue(response.Headers, "X-Location-Summary-Approximated")
        };
    }
    
    private async Task<GeoPagedResultWithMetadataDto<Observation>> SendObservationsBySearchInternalAsync(
        string filterJson,
        ObservationSearchOptions? options,
        CancellationToken cancellationToken)
    {        
        var requestOptions = options ?? new ObservationSearchOptions();
        using var request = new HttpRequestMessage(HttpMethod.Post, BuildObservationsBySearchInternalUri(requestOptions))
        {
            Content = SosClientHttp.CreateJsonContent(filterJson)
        };

        request.ApplySosRequestHeaders(
            requestOptions.RoleId,
            requestOptions.AuthorizationApplicationIdentifier);

        _requestLogging.LogRequest(_httpClient, request, filterJson);
        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        var result = await SosClientHttp.ReadJsonResponseAsync<GeoPagedResultWithMetadataDto<Observation>>(
            response,
            request.RequestUri,
            cancellationToken);

        return result ?? throw SosClientHttp.CreateInvalidResponseException(response, request.RequestUri);
    }

    private async Task<SearchByCursorResultDto<Observation>> SendObservationsByCursorAsync(
        string filterJson,
        SearchByCursorOptions? options,
        CancellationToken cancellationToken)
    {
        var requestOptions = options ?? new SearchByCursorOptions();
        using var request = new HttpRequestMessage(HttpMethod.Post, BuildObservationsByCursorUri(requestOptions))
        {
            Content = SosClientHttp.CreateJsonContent(filterJson)
        };

        request.ApplySosRequestHeaders(
            requestOptions.RoleId,
            requestOptions.AuthorizationApplicationIdentifier);

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        var result = await SosClientHttp.ReadJsonResponseAsync<SearchByCursorResultDto<Observation>>(
            response,
            request.RequestUri,
            cancellationToken);

        return result ?? throw SosClientHttp.CreateInvalidResponseException(response, request.RequestUri);
    }

    private static Uri BuildAdaptiveSearchLocationsInternalUri(AdaptiveSearchLocationsOptions options)
    {
        return new Uri(
            $"observations/internal/adaptivesearchlocations?{options.ToQueryString()}",
            UriKind.Relative);
    }

    private static Uri BuildAdaptiveSearchInternalUri(AdaptiveSearchOptions options)
    {
        return new Uri(
            $"observations/internal/adaptivesearch?{options.ToQueryString()}",
            UriKind.Relative);
    }

    private static Uri BuildObservationsBySearchInternalUri(ObservationSearchOptions options)
    {
        return new Uri($"observations/internal/search?{options.ToQueryString()}", UriKind.Relative);
    }

    private static Uri BuildObservationsByCursorUri(SearchByCursorOptions options)
    {
        return new Uri($"observations/searchbycursor?{options.ToQueryString()}", UriKind.Relative);
    }

    private static Uri BuildCountInternalUri(CountInternalOptions options)
    {
        return new Uri($"observations/internal/count?{options.ToQueryString()}", UriKind.Relative);
    }

    private static long? GetLongHeaderValue(HttpResponseHeaders headers, string name)
    {
        return headers.TryGetValues(name, out var values) &&
               long.TryParse(values.FirstOrDefault(), out var value)
            ? value
            : null;
    }

    private static int? GetIntHeaderValue(HttpResponseHeaders headers, string name)
    {
        return headers.TryGetValues(name, out var values) &&
               int.TryParse(values.FirstOrDefault(), out var value)
            ? value
            : null;
    }

    private static bool? GetBoolHeaderValue(HttpResponseHeaders headers, string name)
    {
        return headers.TryGetValues(name, out var values) &&
               bool.TryParse(values.FirstOrDefault(), out var value)
            ? value
            : null;
    }

    private static string? GetHeaderValue(HttpResponseHeaders headers, string name)
    {
        return headers.TryGetValues(name, out var values)
            ? values.FirstOrDefault()
            : null;
    }

    private static void ThrowIfEmptyOccurrenceId(string occurrenceId)
    {
        if (string.IsNullOrWhiteSpace(occurrenceId))
        {
            throw new ArgumentException("Occurrence id must not be empty.", nameof(occurrenceId));
        }
    }

}
