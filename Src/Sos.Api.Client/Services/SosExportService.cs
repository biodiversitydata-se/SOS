using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Requests;
using SOS.Api.Contracts.Common.Responses;
using SOS.Api.Contracts.Exports.Responses;

namespace SOS.Api.Client.Services;

/// <inheritdoc />
public sealed class SosExportService : ISosExportService
{
    private readonly ISosExportsClient _client;

    /// <summary>Creates the service using the raw export client.</summary>
    public SosExportService(ISosExportsClient client) => _client = client;

    /// <inheritdoc />
    public async Task<PagedResultDto<ExportJobInfoDto>> GetMyExportsPageAsync(
        MyExportsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var requestOptions = options ?? new MyExportsOptions();
        if (requestOptions.Skip < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Skip must be zero or greater.");
        }

        if (requestOptions.Take <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Take must be greater than zero.");
        }

        var exports = await _client.GetMyExportsAsync(requestOptions, cancellationToken);
        var filtered = requestOptions.Statuses is null || requestOptions.Statuses.Count == 0
            ? exports
            : exports.Where(item => requestOptions.Statuses.Contains(item.Status)).ToArray();
        var ordered = filtered.OrderByDescending(item => item.CreatedDate).ToArray();
        return new PagedResultDto<ExportJobInfoDto>
        {
            Skip = requestOptions.Skip,
            Take = requestOptions.Take,
            TotalCount = ordered.Length,
            Records = ordered.Skip(requestOptions.Skip).Take(requestOptions.Take).ToList()
        };
    }
}
