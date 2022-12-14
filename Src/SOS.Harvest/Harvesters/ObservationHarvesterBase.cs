using Microsoft.Extensions.Logging;
using SOS.Harvest.Harvesters.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Harvest.Harvesters
{
    public class ObservationHarvesterBase<TVerbatim, TKey> where TVerbatim : IEntity<TKey>
    {
        private string _provider { get; set; }

        protected IVerbatimRepositoryBase<TVerbatim, TKey> VerbatimRepository { get; private set; }
        protected readonly ILogger<IObservationHarvester> Logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="verbatimRepository"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected ObservationHarvesterBase(
            string provider,
            IVerbatimRepositoryBase<TVerbatim, TKey> verbatimRepository,
            ILogger<IObservationHarvester> logger)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            VerbatimRepository = verbatimRepository ?? throw new ArgumentNullException(nameof(verbatimRepository));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initialize a new harvest
        /// </summary>
        /// <returns></returns>
        protected async Task<long> InitializeHarvestAsync(bool useTempMode)
        {
            Logger.LogInformation($"Start harvesting observations for {_provider} data provider");

            // Get current document count from permanent index
            VerbatimRepository.TempMode = false;
            var preHarvestCount = await VerbatimRepository.CountAllDocumentsAsync();
            VerbatimRepository.TempMode = useTempMode;

            // Make sure we have an empty collection.
            Logger.LogInformation($"Start empty collection for {_provider} verbatim collection");
            await VerbatimRepository.DeleteCollectionAsync();
            await VerbatimRepository.AddCollectionAsync();
            Logger.LogInformation($"Finish empty collection for {_provider} verbatim collection");

            return preHarvestCount;
        }

        /// <summary>
        /// Finish harvest
        /// </summary>
        /// <param name="initValues"></param>
        /// <param name="runStatus"></param>
        /// <param name="harvestCount"></param>
        /// <param name="dataLastModified"></param>
        /// <param name="notes"></param>
        /// <returns></returns>
        protected async Task<HarvestInfo> FinishHarvestAsync((DateTime startDate, long preHarvestCount) initValues,  RunStatus runStatus, int harvestCount, DateTime? dataLastModified = null, string? notes = null)
        {
            // Update harvest info
            var harvestInfo = new HarvestInfo(_provider, initValues.startDate);
            harvestInfo!.End = DateTime.Now;
            harvestInfo!.Count = harvestCount;
            harvestInfo.DataLastModified = dataLastModified;
            harvestInfo.Notes = notes;

            if (VerbatimRepository.TempMode && runStatus == RunStatus.Success)
            {
                if (harvestCount >= initValues.preHarvestCount * 0.8)
                {
                    harvestInfo.Status = RunStatus.Success;
                    Logger.LogInformation($"Start permanentize temp collection for {_provider} verbatim");
                    await VerbatimRepository.PermanentizeCollectionAsync();
                    Logger.LogInformation($"Finish permanentize temp collection for {_provider} verbatim");
                }
                else
                {
                    harvestInfo.Status = RunStatus.Failed;
                    Logger.LogError($"{_provider}: Previous harvested observation count is: {initValues.preHarvestCount}. Now only {harvestCount} observations where harvested.");
                }
            }
            else
            {
                harvestInfo.Status = runStatus;
            }

            Logger.LogInformation($"Finish harvesting {harvestCount} observations for {_provider} data provider. Status: {harvestInfo.Status}");

            // Make sure temp mode is disabled
            VerbatimRepository.TempMode = false;

            return harvestInfo;
        }
    }
}
