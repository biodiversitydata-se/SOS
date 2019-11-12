﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.DarwinCore;
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
        /// <param name="areaHelper"></param>
        /// <param name="processedRepository"></param>
        /// <param name="logger"></param>
        /// <param name="kulObservationVerbatimRepository"></param>
        public KulProcessFactory(
            IKulObservationVerbatimRepository kulObservationVerbatimRepository,
            IAreaHelper areaHelper,
            IProcessedRepository processedRepository,
            ILogger<KulProcessFactory> logger) : base(processedRepository, logger)
        {
            _kulObservationVerbatimRepository = kulObservationVerbatimRepository ?? throw new ArgumentNullException(nameof(kulObservationVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        /// <summary>
        /// Process verbatim data and store it in darwin core format
        /// </summary>
        /// <param name="taxa"></param>
        /// <returns></returns>
        public async Task<bool> ProcessAsync(IDictionary<int, DarwinCoreTaxon> taxa)
        {
            try
            {
                Logger.LogDebug("Start Processing KUL Verbatim observations");

                var verbatim = await _kulObservationVerbatimRepository.GetBatchAsync(0);
                var count = verbatim.Count();

                if (count == 0)
                {
                    Logger.LogError("No verbatim data to process");
                    return false;
                }

                var totalCount = count;

                while (count > 0)
                {
                    var dwcModels = verbatim.ToDarwinCore(taxa)?.ToArray() ?? new DarwinCore<DynamicProperties>[0];

                    // Add area related data to models
                    _areaHelper.AddAreaDataToDarwinCore(dwcModels);

                    await ProcessRepository.AddManyAsync(dwcModels);

                    verbatim = await _kulObservationVerbatimRepository.GetBatchAsync(totalCount + 1);
                    count = verbatim.Count();
                    totalCount += count;
                }

                Logger.LogDebug($"End KUL Verbatim observations process job. Success: true");
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to process KUL Verbatim observations");
                return false;
            }
        }
    }
}
