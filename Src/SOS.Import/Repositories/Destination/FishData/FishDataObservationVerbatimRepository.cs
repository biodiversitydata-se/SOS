using Microsoft.Extensions.Logging;
using SOS.Import.MongoDb.Interfaces;
using SOS.Import.Repositories.Destination.FishData.Interfaces;
using SOS.Lib.Models.Verbatim.FishData;

namespace SOS.Import.Repositories.Destination.FishData
{
    public class FishDataObservationVerbatimRepository : VerbatimRepository<FishDataObservationVerbatim, string>,
        IFishDataObservationVerbatimRepository
    {
        public FishDataObservationVerbatimRepository(
            IImportClient importClient,
            ILogger<FishDataObservationVerbatimRepository> logger) : base(importClient, logger)
        {
        }
    }
}