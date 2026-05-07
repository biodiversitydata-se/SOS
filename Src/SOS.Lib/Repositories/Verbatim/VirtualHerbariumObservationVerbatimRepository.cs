using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Verbatim.VirtualHerbarium;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Lib.Repositories.Verbatim;

/// <summary>
///     Repository for Virtual Herbarium observations
/// </summary>
public class VirtualHerbariumObservationVerbatimRepository :
    VerbatimRepositoryBase<VirtualHerbariumObservationVerbatim, int>,
    IVirtualHerbariumObservationVerbatimRepository
{
    public VirtualHerbariumObservationVerbatimRepository(
        IVerbatimClient importClient,
        ILogger<VirtualHerbariumObservationVerbatimRepository> logger) : base(importClient, logger)
    {
    }
}
