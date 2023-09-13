using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Harvesters.AquaSupport.FishData.Interfaces;
using SOS.Harvest.Harvesters.AquaSupport.Kul.Interfaces;
using SOS.Harvest.Harvesters.AquaSupport.Nors.Interfaces;
using SOS.Harvest.Harvesters.Artportalen.Interfaces;
using SOS.Harvest.Harvesters.Biologg.Interfaces;
using SOS.Harvest.Harvesters.DwC.Interfaces;
using SOS.Harvest.Harvesters.iNaturalist.Interfaces;
using SOS.Harvest.Harvesters.Interfaces;
using SOS.Harvest.Harvesters.Mvm.Interfaces;
using SOS.Harvest.Harvesters.ObservationDatabase.Interfaces;
using SOS.Harvest.Harvesters.Shark.Interfaces;
using SOS.Harvest.Harvesters.VirtualHerbarium.Interfaces;
using SOS.Harvest.HarvestersAquaSupport.Sers.Interfaces;
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
        /// <param name="artportalenObservationHarvester"></param>
        /// <param name="biologgObservationHarvester"></param>
        /// <param name="dwcObservationHarvester"></param>
        /// <param name="fishDataObservationHarvester"></param>
        /// <param name="kulObservationHarvester"></param>
        /// <param name="mvmObservationHarvester"></param>
        /// <param name="norsObservationHarvester"></param>
        /// <param name="observationDatabaseHarvester"></param>
        /// <param name="sersObservationHarvester"></param>
        /// <param name="sharkObservationHarvester"></param>
        /// <param name="virtualHerbariumObservationHarvester"></param>
        /// <param name="iNaturalistObservationHarvester"></param>
        /// <param name="projectHarvester"></param>
        /// <param name="taxonListHarvester"></param>
        /// <param name="dataProviderManager"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public ObservationsHarvestJobIncremental(
            IArtportalenObservationHarvester artportalenObservationHarvester,
            IBiologgObservationHarvester biologgObservationHarvester,
            IDwcObservationHarvester dwcObservationHarvester,
            IFishDataObservationHarvester fishDataObservationHarvester,
            IKulObservationHarvester kulObservationHarvester,
            IMvmObservationHarvester mvmObservationHarvester,
            INorsObservationHarvester norsObservationHarvester,
            IObservationDatabaseHarvester observationDatabaseHarvester,
            ISersObservationHarvester sersObservationHarvester,
            ISharkObservationHarvester sharkObservationHarvester,
            IVirtualHerbariumObservationHarvester virtualHerbariumObservationHarvester,
            IiNaturalistObservationHarvester iNaturalistObservationHarvester,
            IProjectHarvester projectHarvester,
            IArtportalenDatasetMetadataHarvester artportalenDatasetMetadataHarvester,
            ITaxonListHarvester taxonListHarvester,
            IDataProviderManager dataProviderManager,
            IHarvestInfoRepository harvestInfoRepository,
            IProcessObservationsJobIncremental processObservationsJobIncremental,
            ILogger<ObservationsHarvestJobIncremental> logger) : base(artportalenObservationHarvester, biologgObservationHarvester, dwcObservationHarvester, fishDataObservationHarvester, kulObservationHarvester,
                mvmObservationHarvester, norsObservationHarvester, observationDatabaseHarvester, sersObservationHarvester, sharkObservationHarvester, virtualHerbariumObservationHarvester, iNaturalistObservationHarvester,
                projectHarvester, artportalenDatasetMetadataHarvester, taxonListHarvester, dataProviderManager, harvestInfoRepository, logger)
        {
            _processObservationsJobIncremental = processObservationsJobIncremental ?? throw new ArgumentNullException(nameof(processObservationsJobIncremental)); ;
        }

        /// <inheritdoc />
        public async Task<bool> RunIncrementalActiveAsync(DateTime? fromDate, IJobCancellationToken cancellationToken)
        {
            return await HarvestAsync(JobRunModes.IncrementalActiveInstance, fromDate, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> RunIncrementalInactiveAsync(IJobCancellationToken cancellationToken)
        {
            return await HarvestAsync(JobRunModes.IncrementalInactiveInstance, null, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> RunHarvestArtportalenObservationsAsync(
            List<int> sightingIds,
            IJobCancellationToken cancellationToken)
        {
            var harvester = _harvestersByType[DataProviderType.ArtportalenObservations] as IArtportalenObservationHarvester;
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