using CSharpFunctionalExtensions;
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
    public class ObservationsHarvestJobFull : ObservationsHarvestJobBase, IObservationsHarvestJobFull
    {
        private readonly IProcessObservationsJobFull _processObservationsJobFull;
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
        public ObservationsHarvestJobFull(
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
            IProcessObservationsJobFull processObservationsJobFull,
            ILogger<ObservationsHarvestJobFull> logger) : base (artportalenObservationHarvester, biologgObservationHarvester, dwcObservationHarvester, fishDataObservationHarvester, kulObservationHarvester,
                mvmObservationHarvester, norsObservationHarvester, observationDatabaseHarvester, sersObservationHarvester, sharkObservationHarvester, virtualHerbariumObservationHarvester, iNaturalistObservationHarvester, 
                projectHarvester, artportalenDatasetMetadataHarvester, taxonListHarvester, dataProviderManager, harvestInfoRepository, logger)
        {
            _processObservationsJobFull = processObservationsJobFull ?? throw new ArgumentNullException(nameof(processObservationsJobFull));
        }

        /// <inheritdoc />
        public async Task<bool> RunFullAsync(IJobCancellationToken cancellationToken)
        {
            var harvestCount = await RunAsync(JobRunModes.Full, null, cancellationToken);

            // Always process incremental active instance in order to delete removed observations 
            if (harvestCount == 0)
            {
                return true;
            }

            // If harvest was successful, go on with processing
            return await _processObservationsJobFull.RunAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> RunHarvestObservationsAsync(
            List<string> harvestDataProviderIdOrIdentifiers,
            IJobCancellationToken cancellationToken)
        {
            if (harvestDataProviderIdOrIdentifiers == null || harvestDataProviderIdOrIdentifiers.Count == 0)
            {
                _logger.LogInformation(
                    "Couldn't run ObservationHarvestJob because harvestDataProviderIdOrIdentifiers is not set");
                return false;
            }

            var harvestDataProviders =
                await _dataProviderManager.GetDataProvidersByIdOrIdentifier(harvestDataProviderIdOrIdentifiers);
            var harvestDataProvidersResult = Result.Combine(harvestDataProviders);
            if (harvestDataProvidersResult.IsFailure)
            {
                _logger.LogInformation(
                    $"Couldn't run ObservationHarvestJob because of: {harvestDataProvidersResult.Error}");
                return false;
            }

            return (await HarvestAsync(
                harvestDataProviders.Select(d => d.Value).ToList(),
                JobRunModes.Full,
                null,
                cancellationToken)) != -1;
        }
    }
}