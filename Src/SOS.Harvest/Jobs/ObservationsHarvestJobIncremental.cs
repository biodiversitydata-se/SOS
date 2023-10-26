using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Harvesters.Artportalen.Interfaces;
using SOS.Harvest.Harvesters.Interfaces;
using SOS.Harvest.Managers.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Harvest.Jobs
{
    /// <summary>
    ///     Observation harvest job.
    /// </summary>
    public class ObservationsHarvestJobIncremental : ObservationsHarvestJobBase, IObservationsHarvestJobIncremental
    {
        private readonly IProcessObservationsJobIncremental _processObservationsJobIncremental;
        private static SemaphoreSlim incrementalHarvestSemaphore = new SemaphoreSlim(1, 1);

        private async Task<bool> HarvestAsync(JobRunModes mode, DateTime? fromDate, IJobCancellationToken cancellationToken)
        {
            var harvestCount = await RunAsync(mode, fromDate, cancellationToken);

            // Always process incremental active instance in order to delete removed observations 
            if (harvestCount == 0 && mode != JobRunModes.IncrementalActiveInstance)
            {
                return true;
            }

            // If harvest was successful, go on with processing
            return await _processObservationsJobIncremental.RunAsync(
                mode,
                cancellationToken);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observationHarvesterManager"></param>
        /// <param name="projectHarvester"></param>
        /// <param name="artportalenDatasetMetadataHarvester"></param>
        /// <param name="taxonListHarvester"></param>
        /// <param name="dataProviderManager"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="processObservationsJobIncremental"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ObservationsHarvestJobIncremental(
            IObservationHarvesterManager observationHarvesterManager,
            IProjectHarvester projectHarvester,
            IArtportalenDatasetMetadataHarvester artportalenDatasetMetadataHarvester,
            ITaxonListHarvester taxonListHarvester,
            IDataProviderManager dataProviderManager,
            IHarvestInfoRepository harvestInfoRepository,
            IProcessObservationsJobIncremental processObservationsJobIncremental,
            ILogger<ObservationsHarvestJobIncremental> logger) : base(observationHarvesterManager,
                projectHarvester, artportalenDatasetMetadataHarvester, taxonListHarvester, dataProviderManager, harvestInfoRepository, logger)
        {
            _processObservationsJobIncremental = processObservationsJobIncremental ?? throw new ArgumentNullException(nameof(processObservationsJobIncremental)); ;
        }

        /// <inheritdoc />
        public async Task<bool> RunIncrementalActiveAsync(DateTime? fromDate, IJobCancellationToken cancellationToken)
        {
            if (!await incrementalHarvestSemaphore.WaitAsync(0)) return false; // If semaphore is already taken then return false.
            try
            {
                return await HarvestAsync(JobRunModes.IncrementalActiveInstance, fromDate, cancellationToken);
            }
            finally
            {
                incrementalHarvestSemaphore.Release();
            }
        }

        /// <inheritdoc />        
        public async Task<bool> RunIncrementalInactiveAsync(IJobCancellationToken cancellationToken)
        {            
            await incrementalHarvestSemaphore.WaitAsync();
            try
            {
                return await HarvestAsync(JobRunModes.IncrementalInactiveInstance, null, cancellationToken);
            }
            finally
            {                
                incrementalHarvestSemaphore.Release();
            }
        }

        /// <inheritdoc />
        public async Task<bool> RunHarvestArtportalenObservationsAsync(
            List<int> sightingIds,
            IJobCancellationToken cancellationToken)
        {
            var harvester = _observationHarvesterManager.GetHarvester(DataProviderType.ArtportalenObservations) as IArtportalenObservationHarvester;
            var verbatims = await harvester!.HarvestObservationsAsync(sightingIds, cancellationToken);

            if (!verbatims?.Any() ?? true)
            {
                return false;
            }

            // Fix dates
            foreach (var obs in verbatims!)
            {
                if (obs.StartDate.HasValue)
                    obs.StartDate = DateTime.SpecifyKind(obs.StartDate.Value, DateTimeKind.Local);
                if (obs.EndDate.HasValue)
                    obs.EndDate = DateTime.SpecifyKind(obs.EndDate.Value, DateTimeKind.Local);
            }

            return await _processObservationsJobIncremental.ProcessArtportalenObservationsAsync(verbatims);
        }
    }
}