using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Jobs.Process;

namespace SOS.Import.Jobs
{
    /// <summary>
    /// Artportalen harvest
    /// </summary>
    public class ObservationsHarvestJob : IObservationsHarvestJob
    {
        private readonly ITaxonHarvestJob _taxonHarvestJob;
        private readonly IFieldMappingImportJob _fieldMappingImportJob;
        private readonly IArtportalenHarvestJob _artportalenHarvestJob;
        private readonly IClamPortalHarvestJob _clamPortalHarvestJob;
        private readonly IKulHarvestJob _kulHarvestJob;
        private readonly IMvmHarvestJob _mvmHarvestJob;
        private readonly INorsHarvestJob _norsHarvestJob;
        private readonly ISersHarvestJob _sersHarvestJob;
        private readonly ISharkHarvestJob _sharkHarvestJob;
        private readonly IVirtualHerbariumHarvestJob _virtualHerbariumHarvestJob;
        private readonly ILogger<ObservationsHarvestJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="taxonHarvestJob"></param>
        /// <param name="fieldMappingImportJob"></param>
        /// <param name="artportalenHarvestJob"></param>
        /// <param name="clamPortalHarvestJob"></param>
        /// <param name="kulHarvestJob"></param>
        /// <param name="mvmHarvestJob"></param>
        /// <param name="norsHarvestJob"></param>
        /// <param name="sersHarvestJob"></param>
        /// <param name="sharkHarvestJob"></param>
        /// <param name="virtualHerbariumHarvestJob"></param>
        /// <param name="logger"></param>
        public ObservationsHarvestJob(
            ITaxonHarvestJob taxonHarvestJob,
            IFieldMappingImportJob fieldMappingImportJob,
            IArtportalenHarvestJob artportalenHarvestJob,
            IClamPortalHarvestJob clamPortalHarvestJob,
            IKulHarvestJob kulHarvestJob,
            IMvmHarvestJob mvmHarvestJob,
            INorsHarvestJob norsHarvestJob,
            ISersHarvestJob sersHarvestJob,
            ISharkHarvestJob sharkHarvestJob,
            IVirtualHerbariumHarvestJob virtualHerbariumHarvestJob,
            ILogger<ObservationsHarvestJob> logger)
        {
            _taxonHarvestJob = taxonHarvestJob ?? throw new ArgumentNullException(nameof(taxonHarvestJob));
            _fieldMappingImportJob = fieldMappingImportJob ?? throw new ArgumentNullException(nameof(fieldMappingImportJob));
            _artportalenHarvestJob = artportalenHarvestJob ?? throw new ArgumentNullException(nameof(artportalenHarvestJob));
            _clamPortalHarvestJob = clamPortalHarvestJob ?? throw new ArgumentNullException(nameof(clamPortalHarvestJob));
            _kulHarvestJob = kulHarvestJob ?? throw new ArgumentNullException(nameof(kulHarvestJob));
            _mvmHarvestJob = mvmHarvestJob ?? throw new ArgumentNullException(nameof(mvmHarvestJob));
            _norsHarvestJob = norsHarvestJob ?? throw new ArgumentNullException(nameof(norsHarvestJob));
            _sersHarvestJob = sersHarvestJob ?? throw new ArgumentNullException(nameof(sersHarvestJob));
            _sharkHarvestJob = sharkHarvestJob ?? throw new ArgumentNullException(nameof(sharkHarvestJob));
            _virtualHerbariumHarvestJob = virtualHerbariumHarvestJob ?? throw new ArgumentNullException(nameof(virtualHerbariumHarvestJob));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(int harvestSources, int processSources, IJobCancellationToken cancellationToken)
        {
            _logger.LogInformation("Start Harvest Jobs");

            var harvestTasks = new Dictionary<DataSet, Task<bool>>();

            // Always add Artportalen import
            _logger.LogDebug("Added Artportalen harvest (mandatory)");
            harvestTasks.Add(DataSet.ArtportalenObservations, _artportalenHarvestJob.RunAsync(cancellationToken));

            // Always add metadata 
            _logger.LogDebug("Added taxon harvest (mandatory)");
            harvestTasks.Add(DataSet.Taxa, _taxonHarvestJob.RunAsync());
            _logger.LogDebug("Added field mapping harvest (mandatory)");
            harvestTasks.Add(DataSet.FieldMappings, _fieldMappingImportJob.RunAsync());

            // Add Clam portal import if second bit is set
            if ((harvestSources & (int)ObservationProvider.ClamPortal) > 0)
            {
                _logger.LogDebug("Added Clamportal harvest");
                harvestTasks.Add(DataSet.ClamPortalObservations, _clamPortalHarvestJob.RunAsync(cancellationToken));
            }

            // Add KUL import if third bit is set
            if ((harvestSources & (int)ObservationProvider.KUL) > 0)
            {
                _logger.LogDebug("Added KUL harvest");
                harvestTasks.Add(DataSet.KULObservations, _kulHarvestJob.RunAsync(cancellationToken));
            }

            // Add NORS import if fourth bit is set
            if ((harvestSources & (int)ObservationProvider.NORS) > 0)
            {
                _logger.LogDebug("Added NORS harvest");
                harvestTasks.Add(DataSet.NorsObservations, _norsHarvestJob.RunAsync(cancellationToken));
            }

            // Add SERS import if fifth bit is set
            if ((harvestSources & (int)ObservationProvider.SERS) > 0)
            {
                _logger.LogDebug("Added SERS harvest");
                harvestTasks.Add(DataSet.SersObservations, _sersHarvestJob.RunAsync(cancellationToken));
            }

            // Add SHARK import if sixth bit is set
            if ((harvestSources & (int)ObservationProvider.SHARK) > 0)
            {
                _logger.LogDebug("Added SHARK harvest");
                harvestTasks.Add(DataSet.SharkObservations, _sharkHarvestJob.RunAsync(cancellationToken));
            }

            // Add Virtual Herbarium import if eight bit is set
            if ((harvestSources & (int)ObservationProvider.VirtualHerbarium) > 0)
            {
                _logger.LogDebug("Added Virtual Herbarium harvest");
                harvestTasks.Add(DataSet.VirtualHerbariumObservations, _virtualHerbariumHarvestJob.RunAsync(cancellationToken));
            }

            // Add Virtual Herbarium import if night bit is set
            if ((harvestSources & (int)ObservationProvider.MVM) > 0)
            {
                _logger.LogDebug("Added MVM harvest");
                harvestTasks.Add(DataSet.MvmObservations, _mvmHarvestJob.RunAsync(cancellationToken));
            }

            // Run all tasks async
            await Task.WhenAll(harvestTasks.Values);
            _logger.LogInformation("Finish Harvest Jobs");

            // If Artportalen and meta data harvest was successful, go on with processing
            if (harvestTasks[DataSet.ArtportalenObservations].Result &&
                harvestTasks[DataSet.Taxa].Result &&
                harvestTasks[DataSet.FieldMappings].Result)
            {
                // Make sure Artportalen is added 
                if ((processSources & (int)ObservationProvider.Artportalen) == 0)
                {
                    _logger.LogDebug("Added Artportalen to sources to process (mandatory)");
                    processSources += (int)ObservationProvider.Artportalen;
                }

                var jobId = BackgroundJob.Enqueue<IProcessJob>(job => job.RunAsync(processSources, true, true, true, cancellationToken));
               
                _logger.LogInformation($"Start Process Jobs {jobId}");

                return true;
            }

            throw new Exception("Failed to harvest data");
        }
    }
}
