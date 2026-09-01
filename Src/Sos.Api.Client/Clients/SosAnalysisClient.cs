using GeoJSON.Text.Feature;
using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Requests;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Analysis.Responses;

namespace SOS.Api.Client.Clients;

/// <inheritdoc />
public sealed class SosAnalysisClient : ISosAnalysisClient
{
    private readonly HttpClient _httpClient;
    private readonly SosRequestLoggingContext _requestLogging;

    /// <summary>Creates an analysis client using the supplied HTTP client.</summary>
    public SosAnalysisClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _requestLogging = SosRequestLoggingContext.Disabled;
    }

    /// <summary>Creates an analysis client with opt-in request logging.</summary>
    [Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructor]
    public SosAnalysisClient(
        HttpClient httpClient,
        ILogger<SosAnalysisClient> logger,
        IOptions<SosClientOptions> options)
        : this(httpClient)
    {
        _requestLogging = new SosRequestLoggingContext(logger, options);
    }

    /// <inheritdoc />
    public async Task<PagedAggregationResultDto<UserAggregationResponseDto>> AggregateAsync(
        SearchFilterAggregationInternalDto filter,
        FieldAggregationOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.AggregationField))
        {
            throw new ArgumentException("Aggregation field must not be empty.", nameof(options));
        }

        var query = new SosQueryBuilder()
            .Add("aggregationField", options.AggregationField)
            .Add("validateFilter", options.ValidateFilter)
            .Add("aggregateOrganismQuantity", options.AggregateOrganismQuantity)
            .Add("precisionThreshold", options.PrecisionThreshold)
            .Add("afterKey", options.AfterKey)
            .Add("take", options.Take);

        return await SendAsync<PagedAggregationResultDto<UserAggregationResponseDto>>(
                   "/internal/aggregation",
                   query,
                   filter,
                   options,
                   false,
                   cancellationToken)
               ?? new PagedAggregationResultDto<UserAggregationResponseDto>();
    }

    /// <inheritdoc />
    public async Task<FeatureCollection> AggregateAreasAsync(
        SearchFilterAggregationInternalDto filter,
        AreaAggregationOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(options);
        var query = new SosQueryBuilder()
            .Add("areaType", options.AreaType.ToString())
            .Add("coordinateSys", options.CoordinateSystem.HasValue ? (int)options.CoordinateSystem.Value : null)
            .Add("precisionThreshold", options.PrecisionThreshold)
            .Add("aggregateOrganismQuantity", options.AggregateOrganismQuantity)
            .Add("validateFilter", options.ValidateFilter);
        return await SendAsync<FeatureCollection>(
                   "/internal/areas/aggregation", query, filter, options, true, cancellationToken)
               ?? new FeatureCollection();
    }

    /// <inheritdoc />
    public async Task<FeatureCollection> CalculateAooEooAsync(
        SearchFilterAggregationInternalDto filter,
        AooEooOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(options);
        return await SendAsync<FeatureCollection>(
                   "/internal/aoo_eoo",
                   BuildAooEooQuery(options),
                   filter,
                   options,
                   true,
                   cancellationToken)
               ?? new FeatureCollection();
    }

    /// <inheritdoc />
    public async Task<string> OrderAooEooAsync(
        SearchFilterAggregationInternalDto filter,
        AooEooOrderOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(options);
        var query = BuildAooEooQuery(options);
        query.Add("description", options.Description)
            .Add("sendMailFromZendTo", options.SendMailFromZendTo)
            .Add("encryptPassword", options.EncryptPassword)
            .Add("confirmEncryptPassword", options.ConfirmEncryptPassword);
        return await SendAsync<string>(
                   "/internal/order/aoo_eoo", query, filter, options, false, cancellationToken)
               ?? string.Empty;
    }

    /// <inheritdoc />
    public async Task<FeatureCollection> CalculateArticle17Async(
        SearchFilterAggregationInternalDto filter,
        Article17Options options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(options);
        return await SendAsync<FeatureCollection>(
                   "/internal/aoo_eoo/article17",
                   BuildArticle17Query(options),
                   filter,
                   options,
                   true,
                   cancellationToken)
               ?? new FeatureCollection();
    }

    /// <inheritdoc />
    public async Task<string> OrderArticle17Async(
        SearchFilterAggregationInternalDto filter,
        Article17OrderOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(options);
        var query = BuildArticle17Query(options);
        query.Add("description", options.Description)
            .Add("sendMailFromZendTo", options.SendMailFromZendTo)
            .Add("encryptPassword", options.EncryptPassword)
            .Add("confirmEncryptPassword", options.ConfirmEncryptPassword);
        return await SendAsync<string>(
                   "/internal/order/aoo_eoo/article17", query, filter, options, false, cancellationToken)
               ?? string.Empty;
    }

    private async Task<T?> SendAsync<T>(
        string path,
        SosQueryBuilder query,
        SearchFilterAggregationInternalDto filter,
        SosRoleRequestOptions options,
        bool geoJson,
        CancellationToken cancellationToken)
    {
        var filterJson = SosClientHttp.Serialize(filter);
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            query.AppendTo(path.TrimStart('/')))
        {
            Content = SosClientHttp.CreateJsonContent(filterJson)
        };
        request.ApplySosRequestHeaders(
            options.RoleId,
            options.AuthorizationApplicationIdentifier,
            geoJson ? SosAcceptProfile.GeoJson : SosAcceptProfile.Json);
        if (path.Contains("/order/", StringComparison.OrdinalIgnoreCase))
        {
            request.SuppressRetries();
        }

        _requestLogging.LogRequest(_httpClient, request, filterJson);
        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        return await SosClientHttp.ReadJsonResponseAsync<T>(response, request.RequestUri, cancellationToken);
    }

    private static SosQueryBuilder BuildAooEooQuery(AooEooOptions options)
    {
        return new SosQueryBuilder()
            .Add("validateFilter", options.ValidateFilter)
            .AddValues("alphaValues", options.AlphaValues)
            .Add("gridCellSizeInMeters", options.GridCellSizeInMeters)
            .Add("useCenterPoint", options.UseCenterPoint)
            .Add("useEdgeLengthRatio", options.UseEdgeLengthRatio)
            .Add("allowHoles", options.AllowHoles)
            .Add("returnGridCells", options.ReturnGridCells)
            .Add("includeEmptyCells", options.IncludeEmptyCells)
            .Add("metricCoordinateSys", options.MetricCoordinateSystem.HasValue ? (int)options.MetricCoordinateSystem.Value : null)
            .Add("coordinateSystem", options.CoordinateSystem.HasValue ? (int)options.CoordinateSystem.Value : null);
    }

    private static SosQueryBuilder BuildArticle17Query(Article17Options options)
    {
        return new SosQueryBuilder()
            .Add("validateFilter", options.ValidateFilter)
            .Add("maxDistance", options.MaxDistance)
            .Add("gridCellSizeInMeters", options.GridCellSizeInMeters)
            .Add("metricCoordinateSys", options.MetricCoordinateSystem.HasValue ? (int)options.MetricCoordinateSystem.Value : null)
            .Add("coordinateSystem", options.CoordinateSystem.HasValue ? (int)options.CoordinateSystem.Value : null);
    }
}
