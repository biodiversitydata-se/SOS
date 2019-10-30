using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using SOS.Import.MongoDb.Interfaces;
using SOS.Lib.Models.Verbatim.Kul;

namespace SOS.Import.Repositories.Destination.Kul
{
    public class KulSightingVerbatimRepository : VerbatimRepository<KulSightingVerbatim, string>, Interfaces.IKulSightingVerbatimRepository
    {
        public KulSightingVerbatimRepository(
            IImportClient importClient,
            ILogger<KulSightingVerbatimRepository> logger) : base(importClient, logger)
        {
        }
    }
}
