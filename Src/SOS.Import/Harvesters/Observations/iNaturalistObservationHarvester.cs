using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Import.Factories.Harvest;
using SOS.Import.Harvesters.Observations.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Models.Verbatim.Kul;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Import.Harvesters.Observations
{
    public class iNaturalistObservationHarvester : IiNaturalistObservationHarvester
    {
        private readonly IiNaturalistObservationService _iNaturalistObservationService;
        private readonly IDarwinCoreArchiveVerbatimRepository _dwcObservationVerbatimRepository;
        private readonly KulServiceConfiguration _kulServiceConfiguration;
        private readonly ILogger<iNaturalistObservationHarvester> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="kulObservationService"></param>
        /// <param name="dwcObservationVerbatimRepository"></param>
        /// <param name="kulServiceConfiguration"></param>
        /// <param name="logger"></param>
        public iNaturalistObservationHarvester(
            IiNaturalistObservationService kulObservationService,
            IDarwinCoreArchiveVerbatimRepository dwcObservationVerbatimRepository,
            KulServiceConfiguration kulServiceConfiguration,
            ILogger<iNaturalistObservationHarvester> logger)
        {
            _iNaturalistObservationService =
                kulObservationService ?? throw new ArgumentNullException(nameof(kulObservationService));
            _dwcObservationVerbatimRepository = dwcObservationVerbatimRepository ??
                                                throw new ArgumentNullException(
                                                    nameof(dwcObservationVerbatimRepository));
            _kulServiceConfiguration = kulServiceConfiguration ??
                                       throw new ArgumentNullException(nameof(kulServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HarvestInfo> HarvestObservationsAsync(JobRunModes mode, IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo(DateTime.Now);
            var dataProvider = new Lib.Models.Shared.DataProvider() { Id = 19, Identifier = "iNaturalist" };
            try
            {
                _logger.LogInformation("Start harvesting sightings for iNaturalist data provider");
                _logger.LogInformation(GetKulHarvestSettingsInfoString());

                // Make sure we have an empty collection.
                _logger.LogInformation("Start empty collection for iNaturalist verbatim collection");
                await _dwcObservationVerbatimRepository.ClearTempHarvestCollection(dataProvider);
                _logger.LogInformation("Finish empty collection for iNaturalist verbatim collection");

                var nrSightingsHarvested = 0;
                var gBIFResult = await _iNaturalistObservationService.GetAsync(DateTime.Now.AddMonths(-1), DateTime.Now);

                // Loop until all sightings are fetched.
                do
                {
                    // Add sightings to MongoDb
                    await _dwcObservationVerbatimRepository.AddManyToTempHarvestAsync(gBIFResult.Results, dataProvider);

                    nrSightingsHarvested += gBIFResult.Results.Count();

                    _logger.LogDebug($"{ nrSightingsHarvested } iNaturalist observations harvested");

                    cancellationToken?.ThrowIfCancellationRequested();
                    if (_kulServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                        nrSightingsHarvested >= _kulServiceConfiguration.MaxNumberOfSightingsHarvested)
                    {
                        _logger.LogInformation("Max iNaturalist observations reached");
                        break;
                    }

                    gBIFResult = await _iNaturalistObservationService.GetAsync(DateTime.Now.AddMonths(-1), DateTime.Now);
                } while (gBIFResult != null && !gBIFResult.EndOfRecords);

                    _logger.LogInformation("Start permanentize temp collection for iNaturalist verbatim");
                await _dwcObservationVerbatimRepository.RenameTempHarvestCollection(dataProvider);
                _logger.LogInformation("Finish permanentize temp collection for iNaturalist verbatim");

                _logger.LogInformation("Finished harvesting sightings for iNaturalist data provider");

                // Update harvest info
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Success;
                harvestInfo.Count = nrSightingsHarvested;
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("iNaturalist harvest was cancelled.");
                harvestInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to harvest iNaturalist");
                harvestInfo.Status = RunStatus.Failed;
            }

            return harvestInfo;
        }

        private string GetKulHarvestSettingsInfoString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("KUL Harvest settings:");
            sb.AppendLine($"  Start Harvest Year: {_kulServiceConfiguration.StartHarvestYear}");
            sb.AppendLine(
                $"  Max Number Of Sightings Harvested: {_kulServiceConfiguration.MaxNumberOfSightingsHarvested}");
            return sb.ToString();
        }
    }
}