﻿using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Verbatim.FishData;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Lib.Repositories.Verbatim
{
    public class FishDataObservationVerbatimRepository : VerbatimRepositoryBase<FishDataObservationVerbatim, int>,
        IFishDataObservationVerbatimRepository
    {
        public FishDataObservationVerbatimRepository(
            IVerbatimClient importClient,
            ILogger<FishDataObservationVerbatimRepository> logger) : base(importClient, logger)
        {
        }
    }
}