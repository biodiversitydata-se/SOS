using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Requests;
using SOS.Api.Contracts.Aggregations.Requests;

namespace SOS.Api.Client.Services;

/// <inheritdoc />
public sealed class SosAnalysisService : ISosAnalysisService
{
    private readonly ISosAnalysisClient _client;

    /// <summary>Creates the service using the raw analysis client.</summary>
    public SosAnalysisService(ISosAnalysisClient client) => _client = client;

    /// <inheritdoc />
    public Task<string> OrderAooEooAsync(
        SearchFilterAggregationInternalDto filter,
        AooEooOptions calculationOptions,
        SosOrderMetadata order,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(calculationOptions);
        ValidateOrder(order);
        return _client.OrderAooEooAsync(
            filter,
            new AooEooOrderOptions
            {
                RoleId = calculationOptions.RoleId,
                AuthorizationApplicationIdentifier = calculationOptions.AuthorizationApplicationIdentifier,
                ValidateFilter = calculationOptions.ValidateFilter,
                AlphaValues = calculationOptions.AlphaValues,
                GridCellSizeInMeters = calculationOptions.GridCellSizeInMeters,
                UseCenterPoint = calculationOptions.UseCenterPoint,
                UseEdgeLengthRatio = calculationOptions.UseEdgeLengthRatio,
                AllowHoles = calculationOptions.AllowHoles,
                ReturnGridCells = calculationOptions.ReturnGridCells,
                IncludeEmptyCells = calculationOptions.IncludeEmptyCells,
                MetricCoordinateSystem = calculationOptions.MetricCoordinateSystem,
                CoordinateSystem = calculationOptions.CoordinateSystem,
                Description = order.Description,
                SendMailFromZendTo = order.SendMailFromZendTo,
                EncryptPassword = order.EncryptPassword,
                ConfirmEncryptPassword = order.EncryptPassword
            },
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<string> OrderArticle17Async(
        SearchFilterAggregationInternalDto filter,
        Article17Options calculationOptions,
        SosOrderMetadata order,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(calculationOptions);
        ValidateOrder(order);
        return _client.OrderArticle17Async(
            filter,
            new Article17OrderOptions
            {
                RoleId = calculationOptions.RoleId,
                AuthorizationApplicationIdentifier = calculationOptions.AuthorizationApplicationIdentifier,
                ValidateFilter = calculationOptions.ValidateFilter,
                MaxDistance = calculationOptions.MaxDistance,
                GridCellSizeInMeters = calculationOptions.GridCellSizeInMeters,
                MetricCoordinateSystem = calculationOptions.MetricCoordinateSystem,
                CoordinateSystem = calculationOptions.CoordinateSystem,
                Description = order.Description,
                SendMailFromZendTo = order.SendMailFromZendTo,
                EncryptPassword = order.EncryptPassword,
                ConfirmEncryptPassword = order.EncryptPassword
            },
            cancellationToken);
    }

    private static void ValidateOrder(SosOrderMetadata order)
    {
        ArgumentNullException.ThrowIfNull(order);
        if (string.IsNullOrWhiteSpace(order.Description))
        {
            throw new ArgumentException("Description must not be empty.", nameof(order));
        }
    }
}
