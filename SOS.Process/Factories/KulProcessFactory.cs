using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.DarwinCore;
using SOS.Process.Extensions;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Factories
{
    /// <summary>
    /// Process factory class
    /// </summary>
    public class KulProcessFactory : ProcessBaseFactory<KulProcessFactory>, Interfaces.IKulProcessFactory
    {
        private readonly IKulObservationVerbatimRepository _kulObservationVerbatimRepository;
        private readonly IAreaHelper _areaHelper;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="kulObservationVerbatimRepository"></param>
        /// <param name="areaHelper"></param>
        /// <param name="DarwinCoreRepository"></param>
        /// <param name="logger"></param>
        public KulProcessFactory(
            IKulObservationVerbatimRepository kulObservationVerbatimRepository,
            IAreaHelper areaHelper,
            IDarwinCoreRepository DarwinCoreRepository,
            ILogger<KulProcessFactory> logger) : base(DarwinCoreRepository, logger)
        {
            _kulObservationVerbatimRepository = kulObservationVerbatimRepository ?? throw new ArgumentNullException(nameof(kulObservationVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        /// <inheritdoc />
        public async Task<bool> ProcessAsync(
            IDictionary<int, DarwinCoreTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                Logger.LogDebug("Start Processing KUL Verbatim observations");

                if (!await ProcessRepository.DeleteProviderDataAsync(DataProvider.KUL))
                {
                    Logger.LogError("Failed to delete KUL data");

                    return false;
                }

                Logger.LogDebug("Previous processed KUL data deleted");

                var verbatim = await _kulObservationVerbatimRepository.GetBatchAsync(0);
                
                if (!verbatim.Any())
                {
                    Logger.LogError("No verbatim data to process");
                    return false;
                }

                var totalCount = 0;

                while (verbatim.Any())
                {
                    cancellationToken?.ThrowIfCancellationRequested();

                    var darwinCore = verbatim.ToDarwinCore(taxa).ToArray();

                    // Add area related data to models
                    _areaHelper.AddAreaDataToDarwinCore(darwinCore);
                    
                    await ProcessRepository.AddManyAsync(darwinCore);

                    totalCount += verbatim.Count();

                    // Fetch next batch
                    verbatim = await _kulObservationVerbatimRepository.GetBatchAsync(totalCount + 1);
                    
                    Logger.LogInformation($"KUL observations being processed, totalCount: {totalCount}");
                }

                Logger.LogDebug($"End KUL Verbatim observations process job. Success: true");
                return true;
            }
            catch (JobAbortedException)
            {
                Logger.LogInformation("KUL observation processing was canceled.");
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to process KUL Verbatim observations");
                return false;
            }
        }
    }
}
