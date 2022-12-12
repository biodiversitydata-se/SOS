using System.Text;
using System.Xml.Linq;
using DnsClient.Internal;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Harvesters.AquaSupport.Nors.Interfaces;
using SOS.Harvest.Services.Interfaces;
using SOS.Harvesters.AquaSupport;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Nors;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Harvest.Harvesters.AquaSupport.Nors
{
    public class NorsObservationHarvester : ObservationHarvesterBase<NorsObservationVerbatim, int>, INorsObservationHarvester
    {
        private readonly INorsObservationService _norsObservationService;
        private readonly NorsServiceConfiguration _norsServiceConfiguration;

        private string GetNorsHarvestSettingsInfoString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("NORS Harvest settings:");
            sb.AppendLine($"  Start Harvest Year: {_norsServiceConfiguration.StartHarvestYear}");
            if (_norsServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue)
            {
                sb.AppendLine(
                    $"  Max Number Of Sightings Harvested: {_norsServiceConfiguration.MaxNumberOfSightingsHarvested}");
            }
            return sb.ToString();
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="norsObservationService"></param>
        /// <param name="norsObservationVerbatimRepository"></param>
        /// <param name="norsServiceConfiguration"></param>
        /// <param name="logger"></param>
        public NorsObservationHarvester(
            INorsObservationService norsObservationService,
            INorsObservationVerbatimRepository norsObservationVerbatimRepository,
            NorsServiceConfiguration norsServiceConfiguration,
            ILogger<NorsObservationHarvester> logger) : base("Nors", norsObservationVerbatimRepository, logger)
        {
            _norsObservationService =
                norsObservationService ?? throw new ArgumentNullException(nameof(norsObservationService));
            _norsServiceConfiguration = norsServiceConfiguration ??
                                        throw new ArgumentNullException(nameof(norsServiceConfiguration));
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(IJobCancellationToken cancellationToken)
        {
            var runStatus = RunStatus.Success;
            var harvestCount = 0;

            try
            {
                await InitializeharvestAsync(true);
                Logger.LogInformation(GetNorsHarvestSettingsInfoString());

                var ns = (XNamespace) "http://schemas.datacontract.org/2004/07/ArtDatabanken.WebService.Data";
                var verbatimFactory = new AquaSupportHarvestFactory<NorsObservationVerbatim>();
                var startDate = new DateTime(_norsServiceConfiguration.StartHarvestYear, 1, 1);
                var endDate = startDate.AddYears(1).AddDays(-1);
                var changeId = 0L;
                var dataLastModified = DateTime.MinValue;

                var xmlDocument = await _norsObservationService.GetAsync(startDate, endDate, changeId);
                changeId = long.Parse(xmlDocument?.Descendants(ns + "MaxChangeId")?.FirstOrDefault()?.Value ?? "0");

                // Loop until all sightings are fetched.
                while (changeId != 0)
                {
                    var lastRequesetTime = DateTime.Now;

                    Logger.LogDebug(
                            $"Fetching NORS observations between dates {startDate.ToString("yyyy-MM-dd")} and {endDate.ToString("yyyy-MM-dd")}, changeid: {changeId}");

                    var verbatims = await verbatimFactory.CastEntitiesToVerbatimsAsync(xmlDocument);
                    // Clean up
                    xmlDocument = null;

                    // Add sightings to MongoDb
                    await VerbatimRepository.AddManyAsync(verbatims);

                    harvestCount += verbatims.Count();

                    Logger.LogDebug($"{harvestCount} NORS observations harvested");

                    var batchDataLastModified = verbatims.Select(a => a.Modified).Max();

                    if (batchDataLastModified.HasValue && batchDataLastModified.Value > dataLastModified)
                    {
                        dataLastModified = batchDataLastModified.Value;
                    }

                    cancellationToken?.ThrowIfCancellationRequested();
                    if (_norsServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                        harvestCount >= _norsServiceConfiguration.MaxNumberOfSightingsHarvested)
                    {
                        Logger.LogInformation("Max NORS observations reached");
                        break;
                    }

                    var timeSinceLastCall = (DateTime.Now - lastRequesetTime).Milliseconds;
                    if (timeSinceLastCall < 2000)
                    {
                        Thread.Sleep(2000 - timeSinceLastCall);
                    }

                    xmlDocument = await _norsObservationService.GetAsync(startDate, endDate, changeId);
                    changeId = long.Parse(xmlDocument?.Descendants(ns + "MaxChangeId")?.FirstOrDefault()?.Value ?? "0");
                }
            }
            catch (JobAbortedException)
            {
                Logger.LogInformation("NORS harvest was cancelled.");
                runStatus = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to harvest NORS");
                runStatus = RunStatus.Failed;
            }

            return await FinishHarvestAsync(runStatus, harvestCount);
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