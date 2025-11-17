using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Verbatim.INaturalist.Service;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Lib.Repositories.Verbatim;

public class iNaturalistObservationVerbatimRepository :
    VerbatimRepositoryBase<iNaturalistVerbatimObservation, int>,
    IiNaturalistObservationVerbatimRepository
{
    public iNaturalistObservationVerbatimRepository(
        IVerbatimClient importClient,
        ILogger<iNaturalistObservationVerbatimRepository> logger) : base(importClient, "iNaturalistObservations", logger)
    {
    }
}