using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Import.Harvesters.Observations.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Import.Harvesters.Observations
{
    /// <summary>
    ///     Biologg observation harvester
    /// </summary>
    public class BiologObservationHarvester : IBiologObservationHarvester
    {
        private readonly IFileDownloadService _fileDownloadService;
        private readonly IDwcObservationHarvester _dwcObservationHarvester;
        private readonly IDataProviderRepository _dataProviderRepository;
        private readonly BiologConfiguration _biologConfiguration;
        private readonly DwcaConfiguration _dwcaConfiguration;
        private readonly ILogger<BiologObservationHarvester> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileDownloadService"></param>
        /// <param name="dwcObservationHarvester"></param>
        /// <param name="dataProviderRepository"></param>
        /// <param name="biologConfiguration"></param>
        /// <param name="dwcaConfiguration"></param>
        /// <param name="logger"></param>
        public BiologObservationHarvester(
            IFileDownloadService fileDownloadService,
            IDwcObservationHarvester dwcObservationHarvester,
            IDataProviderRepository dataProviderRepository,
            BiologConfiguration biologConfiguration,
            DwcaConfiguration dwcaConfiguration,
            ILogger<BiologObservationHarvester> logger)
        {
            _fileDownloadService = fileDownloadService ?? throw new ArgumentNullException(nameof(fileDownloadService));
            _dwcObservationHarvester = dwcObservationHarvester ??
                                       throw new ArgumentNullException(nameof(dwcObservationHarvester));
            _dataProviderRepository = dataProviderRepository ?? throw new ArgumentNullException(nameof(dataProviderRepository));
            _biologConfiguration = biologConfiguration ?? throw new ArgumentNullException(nameof(biologConfiguration));
            _dwcaConfiguration = dwcaConfiguration ?? throw new ArgumentNullException(nameof(dwcaConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo(DateTime.Now)
            {
                Status = RunStatus.Failed
            };

            try
            {
                var provider = await _dataProviderRepository.GetAsync(18);

                if (provider == null)
                {
                    throw new Exception("Can't load provider info for Biolog");
                }

                _logger.LogInformation("Start requesting for download url for Biolog data provider");

                using var httpClient = new HttpClient();

                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _biologConfiguration.Token);

                var response = await httpClient.GetAsync(_biologConfiguration.Url);
                
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to retrieve download url for Biolog");
                }

                var downLoadUrl = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Finish requesting for download url for Biolog data provider");

                var path = Path.Combine(_dwcaConfiguration.ImportPath, $"dwca-{provider.Identifier}.zip");

                // Try to get DwcA file from download url and store it locally
                if (!await _fileDownloadService.GetFileAndStoreAsync(downLoadUrl, path))
                {
                    return harvestInfo;
                }

                // Harvest file
                var dwcHarvestInfo = await _dwcObservationHarvester.HarvestObservationsAsync(path, provider, cancellationToken);

                // Use harvest info from DWC-harvester, but make sure start time is updated first
                dwcHarvestInfo.Start = harvestInfo.Start;
                harvestInfo = dwcHarvestInfo;

                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (JobAbortedException e)
            {
                _logger.LogError(e, "Canceled harvest of biolog");
                harvestInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed harvest of biolog");
                harvestInfo.Status = RunStatus.Failed;
            }

            return harvestInfo;
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            throw new NotImplementedException("Not implemented for this provider");
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(DataProvider provider, IJobCancellationToken cancellationToken)
        {
            throw new NotImplementedException("Not implemented for this provider");
        }
    }
}