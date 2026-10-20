using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Verbatim.Skogsskada;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Lib.Repositories.Verbatim;

public class SkogsskadaObservationVerbatimRepository : VerbatimRepositoryBase<SkogsskadaObservationVerbatim, int>,
    ISkogsskadaObservationVerbatimRepository
{
    public SkogsskadaObservationVerbatimRepository(
        IVerbatimClient importClient,
        ILogger<SkogsskadaObservationVerbatimRepository> logger) : base(importClient, logger)
    {
    }
}