using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Managers
{
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
                End = processInfo.End,
                Id = processInfo.Id,
                Start = processInfo.Start,
                Status = processInfo.Status.ToString(),
                MetadataInfo = processInfo.MetadataInfo?.Select(mdi => CastToProcessInfoDto(mdi)),
                ProvidersInfo = processInfo.ProvidersInfo?.Select(mdi => CastToProviderInfoDto(mdi))
            };
        }

        private ProviderInfoDto CastToProviderInfoDto(ProviderInfo providerInfo)
        {
            return new ProviderInfoDto
            {
                DataProviderId = providerInfo.DataProviderId,
                DataProviderIdentifier = providerInfo.DataProviderIdentifier,
                HarvestCount = providerInfo.HarvestCount,
                HarvestEnd = providerInfo.HarvestEnd,
                HarvestNotes = providerInfo.HarvestNotes,
                HarvestStart = providerInfo.HarvestStart,
                HarvestStatus = providerInfo.HarvestStatus?.ToString(),
                LatestIncrementalPublicCount = providerInfo.LatestIncrementalPublicCount,
                LatestIncrementalProtectedCount = providerInfo.LatestIncrementalProtectedCount,
                LatestIncrementalEnd = providerInfo.LatestIncrementalEnd,
                LatestIncrementalStart = providerInfo.LatestIncrementalStart,
                LatestIncrementalStatus = providerInfo.LatestIncrementalStatus?.ToString(),
                PublicProcessCount = providerInfo.PublicProcessCount,
                ProtectedProcessCount = providerInfo.ProtectedProcessCount,
                ProcessEnd = providerInfo.ProcessEnd,
                ProcessStart = providerInfo.ProcessStart,
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
}