﻿using Microsoft.Extensions.Logging;
using SOS.Import.MongoDb.Interfaces;
using SOS.Import.Repositories.Destination.Kul.Interfaces;
using SOS.Lib.Models.Verbatim.Kul;

namespace SOS.Import.Repositories.Destination.Kul
{
    public class KulObservationVerbatimRepository : VerbatimRepository<KulObservationVerbatim, string>,
        IKulObservationVerbatimRepository
    {
        public KulObservationVerbatimRepository(
            IImportClient importClient,
            ILogger<KulObservationVerbatimRepository> logger) : base(importClient, logger)
        {
        }
    }
}