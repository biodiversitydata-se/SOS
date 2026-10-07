using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Extensions;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Api.Contracts.Metadata.Responses;
using SOS.Observations.Api.Managers.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Observations.Api.Managers;

/// <summary>
///     Process info manager
/// </summary>
public class ProcessInfoManager : IProcessInfoManager
{
    private readonly ILogger<ProcessInfoManager> _logger;
    private readonly IProcessInfoRepository _processInfoRepository;

    private ProcessInfoDto CastToProcessInfoDto(ProcessInfo processInfo)
    {
        return new ProcessInfoDto
        {
            PublicCount = processInfo.PublicCount,
            ProtectedCount = processInfo.ProtectedCount,
            End = processInfo.End.ToUtcDateTimeOffset(),
            Id = processInfo.Id,
            Start = processInfo.Start.ToUtcDateTimeOffset(),
            Status = processInfo.Status.ToString(),
            MetadataInfo = processInfo.MetadataInfo?.Select(CastToProcessInfoDto).ToList(),
            ProvidersInfo = processInfo.ProvidersInfo?.Select(CastToProviderInfoDto).ToList()
        };
    }

    private ProviderInfoDto CastToProviderInfoDto(ProviderInfo providerInfo)
    {
        return new ProviderInfoDto
        {
            DataProviderId = providerInfo.DataProviderId,
            DataProviderIdentifier = providerInfo.DataProviderIdentifier,
            HarvestCount = providerInfo.HarvestCount,
            HarvestEnd = providerInfo.HarvestEnd.ToUtcDateTimeOffset(),
            HarvestNotes = providerInfo.HarvestNotes,
            HarvestStart = providerInfo.HarvestStart.ToUtcDateTimeOffset(),
            HarvestStatus = providerInfo.HarvestStatus?.ToString(),
            LatestIncrementalPublicCount = providerInfo.LatestIncrementalPublicCount,
            LatestIncrementalProtectedCount = providerInfo.LatestIncrementalProtectedCount,
            LatestIncrementalEnd = providerInfo.LatestIncrementalEnd.ToUtcDateTimeOffset(),
            LatestIncrementalStart = providerInfo.LatestIncrementalStart.ToUtcDateTimeOffset(),
            LatestIncrementalStatus = providerInfo.LatestIncrementalStatus?.ToString(),
            PublicProcessCount = providerInfo.PublicProcessCount,
            ProtectedProcessCount = providerInfo.ProtectedProcessCount,
            ProcessEnd = providerInfo.ProcessEnd.ToUtcDateTimeOffset(),
            ProcessFailCount = providerInfo.ProcessFailCount,
            ProcessStart = providerInfo.ProcessStart.ToUtcDateTimeOffset(),
            ProcessStatus = providerInfo.ProcessStatus?.ToString()
        };
    }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="processInfoRepository"></param>
    /// <param name="logger"></param>
    public ProcessInfoManager(
        IProcessInfoRepository processInfoRepository,
        ILogger<ProcessInfoManager> logger)
    {
        _processInfoRepository = processInfoRepository ??
                                 throw new ArgumentNullException(nameof(processInfoRepository));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<ProcessInfoDto> GetProcessInfoAsync(string id)
    {
        try
        {
            var processInfo = await _processInfoRepository.GetAsync(id);

            return processInfo == null ? null : CastToProcessInfoDto(processInfo);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get current process info");
            return null;
        }
    }
}
