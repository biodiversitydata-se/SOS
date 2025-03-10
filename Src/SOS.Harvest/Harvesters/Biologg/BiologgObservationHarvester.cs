﻿using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Harvesters.Biologg.Interfaces;
using SOS.Harvest.Harvesters.DwC.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Services.Interfaces;
using System.Net.Http.Headers;

namespace SOS.Harvest.Harvesters.Biologg
{
    /// <summary>
    ///     Biologg observation harvester
    /// </summary>
    public class BiologgObservationHarvester : IBiologgObservationHarvester
    {
        private readonly IFileDownloadService _fileDownloadService;
        private readonly IDwcObservationHarvester _dwcObservationHarvester;
        private readonly IDataProviderRepository _dataProviderRepository;
        private readonly BiologgConfiguration _biologgConfiguration;
        private readonly DwcaConfiguration _dwcaConfiguration;
        private readonly ILogger<BiologgObservationHarvester> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileDownloadService"></param>
        /// <param name="dwcObservationHarvester"></param>
        /// <param name="dataProviderRepository"></param>
        /// <param name="biologgConfiguration"></param>
        /// <param name="dwcaConfiguration"></param>
        /// <param name="logger"></param>
        public BiologgObservationHarvester(
            IFileDownloadService fileDownloadService,
            IDwcObservationHarvester dwcObservationHarvester,
            IDataProviderRepository dataProviderRepository,
            BiologgConfiguration biologgConfiguration,
            DwcaConfiguration dwcaConfiguration,
            ILogger<BiologgObservationHarvester> logger)
        {
            _fileDownloadService = fileDownloadService ?? throw new ArgumentNullException(nameof(fileDownloadService));
            _dwcObservationHarvester = dwcObservationHarvester ??
                                       throw new ArgumentNullException(nameof(dwcObservationHarvester));
            _dataProviderRepository = dataProviderRepository ?? throw new ArgumentNullException(nameof(dataProviderRepository));
            _biologgConfiguration = biologgConfiguration ?? throw new ArgumentNullException(nameof(biologgConfiguration));
            _dwcaConfiguration = dwcaConfiguration ?? throw new ArgumentNullException(nameof(dwcaConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }        

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(DataProvider dataProvider,
            JobRunModes mode,
            DateTime? fromDate, IJobCancellationToken cancellationToken)
        {
            await Task.Run(() => throw new NotImplementedException("Not implemented for this provider"));
            return null!;
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(DataProvider provider, IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo("Biologg", DateTime.Now)
            {
                Status = RunStatus.Failed
            };
            _logger.LogInformation($"Start harvesting sightings for Biologg data provider.");

            try
            {
                if (provider == null)
                {
                    throw new Exception("Can't load provider info for Biologg");
                }

                _logger.LogInformation("Start requesting for download url for Biologg data provider");

                using var httpClient = new HttpClient();

                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _biologgConfiguration.Token);

                var response = await httpClient.GetAsync(_biologgConfiguration.Url);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to retrieve download url for Biologg");
                }

                var downLoadUrl = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Finish requesting for download url for Biologg data provider");

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
                _logger.LogError(e, "Canceled harvest of biologg");
                harvestInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed harvest of biologg");
                harvestInfo.Status = RunStatus.Failed;
            }

            _logger.LogInformation($"Finish harvesting sightings for Biologg data provider. Status={harvestInfo.Status}");
            return harvestInfo;
        }

        public Task<HarvestInfo> HarvestCompleteObservationsWithDelayAsync(DataProvider provider, IJobCancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}